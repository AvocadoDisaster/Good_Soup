using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LineRenderer _Line;
    [SerializeField] private float _Step = 0.05f;
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
    private Vector3 lockedTargetPoint;
    private bool hasLockedDirection = false;
    [SerializeField] private float ingredientLifetime = 8f;

    // COOLDOWN SYSTEM
    [SerializeField] private float throwCooldown = 0.5f;
    private float lastThrowTime = -999f;
    private bool isOnCooldown => Time.time < lastThrowTime + throwCooldown;

    // TRAJECTORY - Direct and precise
    private bool isShowingTrajectory = false;
    private Vector3 currentMouseWorldPoint;

    // Prevent multiple throws
    private bool isProcessingThrow = false;

    // AUDIO SYSTEM
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip emberSparkSound;
    [SerializeField] private AudioClip[] grandmaVoiceLines;
    [SerializeField][Range(0f, 1f)] private float grandmaChance = 0.33f;

    private void Start()
    {
        _Camera = Camera.main;
        if (_Line != null)
        {
            _Line.enabled = false;
            _Line.startWidth = 0.15f;
            _Line.endWidth = 0.15f;
            _Line.useWorldSpace = true; // CRITICAL for consistent positioning

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

        // Setup audio source
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

        // HOLD J: Show trajectory - DIRECT, NO SMOOTHING
        if (Input.GetKey(KeyCode.J) && currentHeldEmber != null && !isOnCooldown)
        {
            // Enable line
            if (_Line != null && !isShowingTrajectory)
            {
                _Line.enabled = true;
                isShowingTrajectory = true;
            }

            Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 500f))
            {
                // Store exact mouse hit point
                currentMouseWorldPoint = hit.point;

                // Get ingredient center
                Vector3 ingredientCenter = currentHeldEmber.transform.position;

                // Calculate direction
                Vector3 fullDirection = currentMouseWorldPoint - ingredientCenter;
                Vector3 groundDirection = new Vector3(fullDirection.x, 0, fullDirection.z);

                if (groundDirection.magnitude > 0.1f)
                {
                    Vector3 direction = groundDirection.normalized;
                    lockedThrowDirection = direction;
                    lockedTargetPoint = currentMouseWorldPoint;
                    hasLockedDirection = true;

                    // Calculate trajectory
                    float distance = groundDirection.magnitude;
                    float heightDiff = currentMouseWorldPoint.y - ingredientCenter.y;

                    Vector3 targetPos = new Vector3(distance, heightDiff, 0);
                    float arcHeight = targetPos.y + distance / 2f;
                    arcHeight = Mathf.Max(0.5f, arcHeight);

                    float angle, v0, time;
                    CalculatePathWithH(targetPos, arcHeight, out angle, out v0, out time);

                    // Draw from ingredient center to mouse point
                    DrawPrecisePath(ingredientCenter, direction, v0, angle, time, currentMouseWorldPoint);
                }
            }
        }
        else if (_Line != null && !Input.GetKey(KeyCode.J) && isShowingTrajectory)
        {
            _Line.enabled = false;
            isShowingTrajectory = false;
        }

        // RELEASE J: Throw
        if (Input.GetKeyUp(KeyCode.J))
        {
            if (currentHeldEmber != null && isHoldingJ && hasLockedDirection && !isOnCooldown && !isProcessingThrow)
            {
                isProcessingThrow = true;

                Debug.Log($"<color=cyan>Processing throw for {currentHeldEmber.name}</color>");

                GameObject ingredientToThrow = currentHeldEmber;
                Vector3 throwStartPos = ingredientToThrow.transform.position;
                Vector3 throwDirection = lockedThrowDirection;
                Vector3 targetPoint = lockedTargetPoint;

                // Calculate final trajectory
                Vector3 toTarget = targetPoint - throwStartPos;
                Vector3 groundDir = new Vector3(toTarget.x, 0, toTarget.z);
                float distance = groundDir.magnitude;
                float heightDiff = targetPoint.y - throwStartPos.y;

                Vector3 targetPos = new Vector3(distance, heightDiff, 0);
                float arcHeight = targetPos.y + distance / 2f;
                arcHeight = Mathf.Max(0.5f, arcHeight);

                float angle, v0, time;
                CalculatePathWithH(targetPos, arcHeight, out angle, out v0, out time);

                // Mark as thrown
                Projectile proj = ingredientToThrow.GetComponent<Projectile>();
                if (proj != null)
                    proj.hasBeenThrown = true;

                // SET COOLDOWN
                lastThrowTime = Time.time;

                // Clear state
                currentHeldEmber = null;
                isHoldingJ = false;
                hasLockedDirection = false;
                isShowingTrajectory = false;

                if (_Line != null)
                {
                    _Line.enabled = false;
                }

                // Clean up components
                LineRenderer ingredientLine = ingredientToThrow.GetComponent<LineRenderer>();
                if (ingredientLine != null)
                {
                    ingredientLine.enabled = false;
                    Destroy(ingredientLine);
                }

                Projectile ingredientProjectile = ingredientToThrow.GetComponent<Projectile>();
                if (ingredientProjectile != null && ingredientProjectile != this)
                {
                    Destroy(ingredientProjectile);
                }

                // Unparent
                ingredientToThrow.transform.SetParent(null);

                // Setup for flight
                Collider ingredientCol = ingredientToThrow.GetComponent<Collider>();
                Rigidbody ingredientRb = ingredientToThrow.GetComponent<Rigidbody>();

                if (ingredientCol != null) ingredientCol.enabled = false;
                if (ingredientRb != null)
                {
                    ingredientRb.isKinematic = true;
                    ingredientRb.useGravity = false;
                }

                Debug.Log($"<color=green>✓ Throwing {ingredientToThrow.name} to {targetPoint}</color>");

                // Play sounds
                PlayThrowSounds(ingredientToThrow);

                // Clean up old controller
                ThrowController oldController = ingredientToThrow.GetComponent<ThrowController>();
                if (oldController != null)
                {
                    Destroy(oldController);
                }

                // Add controller
                ThrowController controller = ingredientToThrow.AddComponent<ThrowController>();

                // Start throw
                StartCoroutine(ExecuteThrowSequence(controller, throwStartPos, throwDirection, v0, angle, time, ingredientLifetime, ingredientToThrow));

                IsThrown = true;
            }
            else if (isOnCooldown)
            {
                Debug.Log($"<color=orange>Cannot throw - on cooldown!</color>");
            }
        }
    }

    private void DrawPrecisePath(Vector3 startPos, Vector3 direction, float v0, float angle, float time, Vector3 targetPoint)
    {
        if (_Line == null || !_Line.enabled) return;

        int segments = Mathf.Max(15, (int)(time / _Step));
        _Line.positionCount = segments + 1;

        // Draw arc
        for (int i = 0; i <= segments; i++)
        {
            float t = (time / segments) * i;
            float x = v0 * t * Mathf.Cos(angle);
            float y = v0 * t * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(t, 2);

            Vector3 point = startPos + direction * x + Vector3.up * y;
            _Line.SetPosition(i, point);
        }

        // Force last point to exact target
        _Line.SetPosition(segments, targetPoint);
    }

    private IEnumerator ExecuteThrowSequence(ThrowController controller, Vector3 startPos, Vector3 direction, float v0, float angle, float time, float lifetime, GameObject thrownObject)
    {
        yield return null;

        if (controller != null && thrownObject != null)
        {
            controller.StartThrow(startPos, direction, v0, angle, time, lifetime);
            yield return null;

            if (spawner != null)
            {
                Debug.Log("<color=cyan>Spawning next ingredient...</color>");
                spawner.SpawnIngredient();
            }
        }
        else
        {
            if (spawner != null)
            {
                spawner.SpawnIngredient();
            }
        }

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
            ThrowController tc = col.GetComponent<ThrowController>();
            Projectile proj = col.GetComponent<Projectile>();

            if (proj != null && proj.HasBeenThrown)
                continue;

            if (tc != null && tc.IsFlying)
                continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col.gameObject;
            }
        }

        return nearest;
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

        ThrowController controller = Ember.GetComponent<ThrowController>();
        if (controller != null)
        {
            controller.StopThrow();
            Destroy(controller);
        }

        Projectile emberProjectile = Ember.GetComponent<Projectile>();
        if (emberProjectile == null)
        {
            emberProjectile = Ember.AddComponent<Projectile>();
            emberProjectile.SetSpoon(Spoon);
            emberProjectile.spawner = this.spawner;
            Debug.Log($"  - Re-added Projectile script to {Ember.name}");
        }

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
                emberLine.useWorldSpace = true;
            }

            Debug.Log($"  - Re-added LineRenderer to {Ember.name}");
        }
        emberLine.enabled = false;

        Ember.transform.SetParent(Spoon);
        Ember.transform.localPosition = Vector3.zero;
        Ember.transform.localRotation = Quaternion.identity;

        Rigidbody emberRb = Ember.GetComponent<Rigidbody>();
        if (emberRb != null)
        {
            emberRb.linearVelocity = Vector3.zero;
            emberRb.angularVelocity = Vector3.zero;
            emberRb.isKinematic = true;
            emberRb.useGravity = false;
        }

        Collider emberCol = Ember.GetComponent<Collider>();
        if (emberCol != null) emberCol.enabled = false;
    }

    internal void SetSpoon(Transform spoon)
    {
        Spoon = spoon.transform;
    }

    public float GetCooldownProgress()
    {
        if (!isOnCooldown) return 1f;
        return (Time.time - lastThrowTime) / throwCooldown;
    }

    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    private void PlayThrowSounds(GameObject thrownObject)
    {
        if (audioSource == null) return;

        if (throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
            Debug.Log("<color=cyan> Playing throw sound</color>");
        }

        if (thrownObject.CompareTag("Ember") && emberSparkSound != null)
        {
            audioSource.PlayOneShot(emberSparkSound);
            Debug.Log("<color=orange> Playing ember spark sound</color>");
        }

        if (grandmaVoiceLines != null && grandmaVoiceLines.Length > 0)
        {
            float randomValue = UnityEngine.Random.Range(0f, 1f);
            if (randomValue <= grandmaChance)
            {
                AudioClip randomVoiceLine = grandmaVoiceLines[UnityEngine.Random.Range(0, grandmaVoiceLines.Length)];
                if (randomVoiceLine != null)
                {
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