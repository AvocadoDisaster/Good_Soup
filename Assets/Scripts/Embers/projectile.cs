using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LineRenderer _Line;
    [SerializeField] private float _Step = 0.1f;
    private Transform Spoon;
    public bool IsThrown;
    private bool hasBeenThrown = false;
    public bool HasBeenThrown => hasBeenThrown;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Camera _Camera;

    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.1f;

    [SerializeField] private float indicatorHoverHeight = 0.2f;
    public GameObject currentHeldEmber;
    [SerializeField] private Vector3 lineOriginOffset = Vector3.zero;

    private bool isHoldingJ = false;

    [SerializeField] private float groundCheckRadius = 1f;
    [SerializeField] public ItemSpawner spawner;
    private Rigidbody rb;

    private Vector3 lockedThrowDirection;
    private bool hasLockedDirection = false;
    [SerializeField] private float ingredientLifetime = 8f;

    // COOLDOWN SYSTEM
    [SerializeField] private float throwCooldown = 0.5f;
    private float lastThrowTime = -999f;
    private bool isOnCooldown => Time.time < lastThrowTime + throwCooldown;

    // TRAJECTORY SMOOTHING - Simple and responsive
    private bool isShowingTrajectory = false;
    private Vector3 lastMouseDirection;
    [SerializeField] private float lineResponseSpeed = 0.05f; // Lower = more responsive (0.02-0.15)

    // NEW: Prevent multiple throws
    private bool isProcessingThrow = false;

    // AUDIO SYSTEM
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource; // Main audio source
    [SerializeField] private AudioClip throwSound; // General throwing sound
    [SerializeField] private AudioClip emberSparkSound; // Special sound for embers
    [SerializeField] private AudioClip[] grandmaVoiceLines; // Array of grandma voice lines
    [SerializeField][Range(0f, 1f)] private float grandmaChance = 0.33f; // 1 in 3 chance

    private void Start()
    {
        _Camera = Camera.main;
        if (_Line != null)
        {
            _Line.enabled = false;
            _Line.startWidth = 0.15f;
            _Line.endWidth = 0.15f;

            if (_Line.material == null || _Line.material.shader.name != "Sprites/Default")
            {
                _Line.material = new Material(Shader.Find("Sprites/Default"));
            }
            _Line.material.color = new Color(1f, 0.8f, 0f, 1f);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.yellow, 0.0f),
                    new GradientColorKey(Color.red, 1.0f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f)
                }
            );
            _Line.colorGradient = gradient;
        }
        spawner = FindFirstObjectByType<ItemSpawner>();
        rb = GetComponent<Rigidbody>();

        if (Spoon == null)
        {
            Spoon = GameObject.Find("Spoon").transform;
            Debug.LogWarning("Spoon reference not set — auto-found at runtime!");
        }

        // Setup audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void Update()
    {
        // PRESS J: Pick up nearest ingredient
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("<color=yellow>J Key Pressed Down</color>");

            // Check cooldown
            if (isOnCooldown)
            {
                Debug.Log($"<color=orange>On cooldown! {(lastThrowTime + throwCooldown - Time.time):F1}s remaining</color>");
                return;
            }

            if (currentHeldEmber == null)
            {
                GameObject nearest = FindNearestEmber();
                if (nearest != null)
                {
                    currentHeldEmber = nearest;
                    _Holding(currentHeldEmber);
                    isHoldingJ = true;
                    hasLockedDirection = false;
                    isShowingTrajectory = false;

                    Debug.Log($"<color=green>Picked up {currentHeldEmber.name}</color>");
                }
                else
                {
                    Debug.Log("<color=red>No ember or ingredient nearby!</color>");
                }
            }
        }

        // HOLD J: Show trajectory and UPDATE direction continuously (follows mouse)
        if (Input.GetKey(KeyCode.J) && currentHeldEmber != null && !isOnCooldown)
        {
            // Enable line once when starting to show trajectory
            if (_Line != null && !isShowingTrajectory)
            {
                _Line.enabled = true;
                isShowingTrajectory = true;
                lastMouseDirection = Vector3.zero;
            }

            Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Raycast against EVERYTHING to get smooth updates
            if (Physics.Raycast(ray, out hit, 500f))
            {
                Vector3 direction = hit.point - Spoon.position;
                Vector3 groundDirection = new Vector3(direction.x, 0, direction.z);

                if (groundDirection.magnitude > 0.1f)
                {
                    Vector3 targetDirection = groundDirection.normalized;

                    // Very light smoothing for anti-jitter
                    if (lastMouseDirection == Vector3.zero)
                    {
                        lastMouseDirection = targetDirection;
                    }
                    else
                    {
                        // Interpolate with high responsiveness
                        lastMouseDirection = Vector3.Lerp(lastMouseDirection, targetDirection, 1f - lineResponseSpeed);
                    }

                    lockedThrowDirection = lastMouseDirection;
                    hasLockedDirection = true;

                    Vector3 targetPos = new Vector3(groundDirection.magnitude, direction.y, 0);
                    float height = targetPos.y + targetPos.magnitude / 2f;
                    height = Mathf.Max(0.01f, height);

                    float angle, v0, time;
                    CalculatePathWithH(targetPos, height, out angle, out v0, out time);
                    DrawPath(lastMouseDirection, v0, angle, time, _Step);
                }
            }
        }
        else if (_Line != null && !Input.GetKey(KeyCode.J) && isShowingTrajectory)
        {
            _Line.enabled = false;
            isShowingTrajectory = false;
        }

        // RELEASE J: Throw ingredient with FINAL locked direction
        if (Input.GetKeyUp(KeyCode.J))
        {
            if (currentHeldEmber != null && isHoldingJ && hasLockedDirection && !isOnCooldown && !isProcessingThrow)
            {
                isProcessingThrow = true; // Prevent multiple throws

                Debug.Log($"<color=cyan>Processing throw for {currentHeldEmber.name}</color>");

                Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Raycast against everything for final throw calculation
                if (Physics.Raycast(ray, out hit, 1000f))
                {
                    Vector3 direction = hit.point - Spoon.position;
                    Vector3 groundDirection = new Vector3(direction.x, 0, direction.z);

                    if (groundDirection.magnitude > 0.1f)
                    {
                        Vector3 targetPos = new Vector3(groundDirection.magnitude, direction.y, 0);
                        float height = targetPos.y + targetPos.magnitude / 2f;
                        height = Mathf.Max(0.01f, height);

                        float angle, v0, time;
                        CalculatePathWithH(targetPos, height, out angle, out v0, out time);

                        // Capture all data BEFORE making changes
                        GameObject ingredientToThrow = currentHeldEmber;
                        

                        // Mark this ingredient permanently unthrowable
                        Projectile proj = ingredientToThrow.GetComponent<Projectile>();
                        if (proj != null)
                            proj.hasBeenThrown = true;

                        Vector3 throwStartPos = Spoon.position;
                        Vector3 throwDirection = lastMouseDirection; // Use the smoothed direction from display

                        // SET COOLDOWN TIMER
                        lastThrowTime = Time.time;

                        // Clear player state FIRST
                        currentHeldEmber = null;
                        isHoldingJ = false;
                        hasLockedDirection = false;
                        isShowingTrajectory = false;

                        // Hide line renderer
                        if (_Line != null)
                        {
                            _Line.enabled = false;
                        }

                        // Clean up ingredient's components
                        LineRenderer ingredientLine = ingredientToThrow.GetComponent<LineRenderer>();
                        if (ingredientLine != null)
                        {
                            ingredientLine.enabled = false;
                            Destroy(ingredientLine);
                            Debug.Log($"  - DESTROYED LineRenderer on {ingredientToThrow.name}");
                        }

                        Projectile ingredientProjectile = ingredientToThrow.GetComponent<Projectile>();
                        if (ingredientProjectile != null && ingredientProjectile != this)
                        {
                            Destroy(ingredientProjectile);
                            Debug.Log($"  - DESTROYED Projectile script on {ingredientToThrow.name}");
                        }

                        // Unparent ingredient
                        ingredientToThrow.transform.SetParent(null);

                        // Setup for flight
                        Collider ingredientCol = ingredientToThrow.GetComponent<Collider>();
                        Rigidbody ingredientRb = ingredientToThrow.GetComponent<Rigidbody>();

                        if (ingredientCol != null)
                        {
                            ingredientCol.enabled = false;
                        }

                        if (ingredientRb != null)
                        {
                            ingredientRb.isKinematic = true;
                            ingredientRb.useGravity = false;
                        }

                        Debug.Log($"<color=green>✓ Throwing {ingredientToThrow.name}!</color>");
                        Debug.Log($"  From: {throwStartPos}, Dir: {throwDirection}");

                        // PLAY THROW SOUND EFFECTS
                        PlayThrowSounds(ingredientToThrow);

                        // Clean up old ThrowController
                        ThrowController oldController = ingredientToThrow.GetComponent<ThrowController>();
                        if (oldController != null)
                        {
                            Debug.LogWarning($"Found existing ThrowController on {ingredientToThrow.name}, destroying it!");
                            Destroy(oldController);
                        }

                        // Add fresh controller
                        ThrowController controller = ingredientToThrow.AddComponent<ThrowController>();

                        // Start throw and spawn AFTER throw is set up
                        StartCoroutine(ExecuteThrowSequence(controller, throwStartPos, throwDirection, v0, angle, time, ingredientLifetime, ingredientToThrow));

                        IsThrown = true;
                    }
                    else
                    {
                        Debug.LogWarning("Target too close to throw!");
                        hasLockedDirection = false;
                        isShowingTrajectory = false;
                        isProcessingThrow = false;
                        if (_Line != null) _Line.enabled = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Raycast didn't hit anything!");
                    hasLockedDirection = false;
                    isShowingTrajectory = false;
                    isProcessingThrow = false;
                    if (_Line != null) _Line.enabled = false;
                }
            }
            else if (isOnCooldown)
            {
                Debug.Log($"<color=orange>Cannot throw - on cooldown!</color>");
            }
        }
    }

    private IEnumerator ExecuteThrowSequence(ThrowController controller, Vector3 startPos, Vector3 direction, float v0, float angle, float time, float lifetime, GameObject thrownObject)
    {
        // Wait one frame for component initialization
        yield return null;

        if (controller != null && thrownObject != null)
        {
            // Start the throw
            controller.StartThrow(startPos, direction, v0, angle, time, lifetime);

            // Wait another frame to ensure throw has started
            yield return null;

            // Now spawn the next ingredient
            if (spawner != null)
            {
                Debug.Log("<color=cyan>Spawning next ingredient...</color>");
                spawner.SpawnIngredient();
            }
            else
            {
                Debug.LogError("Spawner reference is null!");
            }
        }
        else
        {
            Debug.LogError("ThrowController or ingredient was destroyed before throw could complete!");

            // Still try to spawn if something went wrong
            if (spawner != null)
            {
                spawner.SpawnIngredient();
            }
        }

        // Reset throw processing flag after a small delay
        yield return new WaitForSeconds(0.2f);
        isProcessingThrow = false;
    }

    private GameObject FindNearestEmber()
    {
        float detectRadius = 2.5f;
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, detectRadius);

        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider col in nearbyObjects)
        {
            if (!col.CompareTag("Ember") && !col.CompareTag("Ingredient"))
                continue;

            

            // Don't pick up ingredients that are currently flying
            ThrowController tc = col.GetComponent<ThrowController>(); // NEVER pick up ingredients that have already been thrown
            Projectile proj = col.GetComponent<Projectile>();
            if (proj != null && proj.HasBeenThrown)
                continue;

            if (tc != null && tc.IsFlying)
                continue;
            // IMPORTANT: Don't pick up trash items!
            if (col.gameObject.name.ToLower().Contains("trash") && tc.IsFlying)
            {
                Debug.Log($"<color=yellow>Skipping trash item: {col.gameObject.name}</color>");
                continue;
            }

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col.gameObject;
            }
        }

        return nearest;
    }

    private void DrawPath(Vector3 direction, float v0, float angle, float time, float step)
    {
        if (_Line == null || !_Line.enabled) return;

        Vector3 origin = currentHeldEmber != null ?
            currentHeldEmber.transform.position + lineOriginOffset :
            Spoon.position;

        step = Mathf.Max(0.01f, step);
        int posCount = (int)(time / step) + 2;

        // Set position count once
        if (_Line.positionCount != posCount)
        {
            _Line.positionCount = posCount;
        }

        int count = 0;

        // Draw the path
        for (float i = 0; i < time; i += step)
        {
            float x = v0 * i * Mathf.Cos(angle);
            float y = v0 * i * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(i, 2);

            if (count < _Line.positionCount)
            {
                _Line.SetPosition(count, origin + direction * x + Vector3.up * y);
                count++;
            }
        }

        // Final position
        if (count < _Line.positionCount)
        {
            float xf = v0 * time * Mathf.Cos(angle);
            float yf = v0 * time * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(time, 2);
            _Line.SetPosition(count, origin + direction * xf + Vector3.up * yf);
        }
    }

    private float QuadraticEquation(float a, float b, float c, float sign)
    {
        return (-b + sign * Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
    }

    private void CalculatePathWithH(Vector3 targetPos, float h, out float angle, out float v0, out float time)
    {
        float xt = targetPos.x;
        float yt = targetPos.y;
        float g = -Physics.gravity.y;

        float b = Mathf.Sqrt(2 * g * h);
        float a = (-0.5f * g);
        float c = -yt;

        float tplus = QuadraticEquation(a, b, c, 1);
        float tmin = QuadraticEquation(a, b, c, -1);
        time = tplus > tmin ? tplus : tmin;

        angle = Mathf.Atan(b * time / xt);
        v0 = b / Mathf.Sin(angle);
    }

    public void _Holding(GameObject Ember)
    {
        if (Ember == null) return;

        IsThrown = false;

        // Stop any active throw controller
        ThrowController controller = Ember.GetComponent<ThrowController>();
        if (controller != null)
        {
            controller.StopThrow();
            Destroy(controller);
        }

        // Check if this ingredient needs a Projectile script and LineRenderer
        Projectile emberProjectile = Ember.GetComponent<Projectile>();
        if (emberProjectile == null)
        {
            emberProjectile = Ember.AddComponent<Projectile>();
            emberProjectile.SetSpoon(Spoon);
            emberProjectile.spawner = this.spawner;
            Debug.Log($"  - Re-added Projectile script to {Ember.name}");
        }

        // Check if LineRenderer exists, if not add it
        LineRenderer emberLine = Ember.GetComponent<LineRenderer>();
        if (emberLine == null)
        {
            emberLine = Ember.AddComponent<LineRenderer>();

            if (_Line != null)
            {
                emberLine.startWidth = _Line.startWidth;
                emberLine.endWidth = _Line.endWidth;
                emberLine.material = new Material(_Line.material);
                emberLine.colorGradient = _Line.colorGradient;
            }

            Debug.Log($"  - Re-added LineRenderer to {Ember.name}");
        }
        emberLine.enabled = false;

        // IMPORTANT: Set parent and position BEFORE physics setup
        Ember.transform.SetParent(Spoon);
        Ember.transform.localPosition = Vector3.zero;
        Ember.transform.localRotation = Quaternion.identity;

        // Now handle physics - this prevents falling through floor
        Rigidbody emberRb = Ember.GetComponent<Rigidbody>();
        if (emberRb != null)
        {
            // Stop all physics immediately
            emberRb.linearVelocity = Vector3.zero;
            emberRb.angularVelocity = Vector3.zero;
            emberRb.isKinematic = true;
            emberRb.useGravity = false;
        }

        // Disable collider to prevent physics interactions
        Collider emberCol = Ember.GetComponent<Collider>();
        if (emberCol != null) emberCol.enabled = false;
    }

    internal void SetSpoon(Transform spoon)
    {
        Spoon = spoon.transform;
    }

    // Optional: Visual feedback for cooldown
    public float GetCooldownProgress()
    {
        if (!isOnCooldown) return 1f;
        return (Time.time - lastThrowTime) / throwCooldown;
    }

    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    // AUDIO METHODS
    private void PlayThrowSounds(GameObject thrownObject)
    {
        if (audioSource == null) return;

        // 1. Always play the throwing sound
        if (throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
            Debug.Log("<color=cyan>🎵 Playing throw sound</color>");
        }

        // 2. Check if it's an ember and play spark sound
        if (thrownObject.CompareTag("Ember") && emberSparkSound != null)
        {
            audioSource.PlayOneShot(emberSparkSound);
            Debug.Log("<color=orange> Playing ember spark sound</color>");
        }

        // 3. Random chance to play grandma voice line (1 in 3 chance)
        if (grandmaVoiceLines != null && grandmaVoiceLines.Length > 0)
        {
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            if (randomValue <= grandmaChance)
            {
                // Pick a random voice line
                AudioClip randomVoiceLine = grandmaVoiceLines[UnityEngine.Random.Range(0, grandmaVoiceLines.Length)];
                if (randomVoiceLine != null)
                {
                    // Play with slight delay so it doesn't overlap too much with throw sound
                    StartCoroutine(PlayDelayedSound(randomVoiceLine, 0.1f));
                    Debug.Log("<color=magenta> Playing grandma voice line!</color>");
                }
            }
        }
    }

    private IEnumerator PlayDelayedSound(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}