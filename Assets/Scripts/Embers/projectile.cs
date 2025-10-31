using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.VisualScripting;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LineRenderer _Line;
    [SerializeField] private float _Step = 0.1f;
    [SerializeField] private Transform Spoon;
    public bool IsThrown;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Camera _Camera;
    [SerializeField] private InParty InParty;
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.1f;
    [SerializeField] private GameObject landingIndicator; // Visual marker for landing spot
    [SerializeField] private float indicatorHoverHeight = 0.2f;
    private GameObject currentHeldEmber = null;
    private bool isHoldingJ = false;
    private Vector3 predictedLandingPoint;
    private bool validLandingSpot = false;
    [SerializeField] private float groundCheckRadius = 1f;
    [SerializeField] private float maxGroundSearchDistance = 10f;
    [SerializeField] private float landingSnapSpeed = 20f;

    private void Start()
    {
        _Camera = Camera.main;
        if (_Line != null)
        {
            _Line.enabled = false;
        }
        if (landingIndicator != null)
        {
            landingIndicator.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Landing Indicator not assigned! Creating default sphere...");
            CreateDefaultLandingIndicator();
        }
    }
    private void CreateDefaultLandingIndicator()
    {
        // Create a simple sphere as landing indicator
        landingIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        landingIndicator.name = "LandingIndicator";
        landingIndicator.transform.localScale = Vector3.one * 0.5f;

        // Remove collider
        Destroy(landingIndicator.GetComponent<Collider>());

        // Make it glow
        Renderer rend = landingIndicator.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Standard"));
        rend.material.SetColor("_Color", Color.yellow);
        rend.material.SetFloat("_Metallic", 0.5f);
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", Color.yellow);

        landingIndicator.SetActive(true);
    }

    private void Update()
    {
        // DEBUG: Show party count
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"<color=white>Party Count: {InParty.InCurrentParty.Count}</color>");
            for (int i = 0; i < InParty.InCurrentParty.Count; i++)
            {
                Debug.Log($"  [{i}] {InParty.InCurrentParty[i].name}");
            }
        }

        // Check if we have embers in the party
        if (InParty.InCurrentParty.Count == 0 && currentHeldEmber == null)
        {
            if (_Line != null) _Line.enabled = false;
            return;
        }

        // PRESS J: Pick up the first ember from party
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log($"<color=yellow>J Key Pressed Down!</color>");
            Debug.Log($"  - Current Held Ember: {(currentHeldEmber == null ? "NULL" : currentHeldEmber.name)}");
            Debug.Log($"  - Party Count: {InParty.InCurrentParty.Count}");

            if (currentHeldEmber == null && InParty.InCurrentParty.Count > 0)
            {
                GameObject emberToHold = InParty.InCurrentParty[0];

                Debug.Log($"<color=yellow>Attempting to hold ember: {emberToHold.name}</color>");

                // Check current state
                EmberState emberCurrentState = EmberStateManager.Instance.GetEmberState(emberToHold);
                Debug.Log($"  - Ember current state: {emberCurrentState}");

                // Claim the ember for throwing
                bool claimed = EmberStateManager.Instance.TryClaimEmber(emberToHold, EmberState.BEING_THROWN);
                Debug.Log($"  - Claim result: {claimed}");

                if (claimed)
                {
                    InParty.InCurrentParty.RemoveAt(0); // REMOVE FROM PARTY FIRST
                    Debug.Log($"  - Removed from party. New count: {InParty.InCurrentParty.Count}");

                    // FIXED: Unparent BEFORE holding to prevent staying as child
                    emberToHold.transform.SetParent(null);

                    _Holding(emberToHold);
                    currentHeldEmber = emberToHold;
                    isHoldingJ = true;
                    Debug.Log($"<color=green>✓ Holding Ember: {emberToHold.name}</color>");
                }
                else
                {
                    Debug.LogError($"<color=red>❌ Failed to claim ember {emberToHold.name} - state: {emberCurrentState}</color>");
                }
            }
        }

        // HOLD J: Show trajectory line
        if (Input.GetKey(KeyCode.J) && currentHeldEmber != null)
        {
            if (_Line != null) _Line.enabled = true;

            Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 500f)) // Added max distance
            {
                Vector3 direction = hit.point - Spoon.position;
                Vector3 groundDirection = new Vector3(direction.x, 0, direction.z);

                if (groundDirection.magnitude > 0.1f) // Minimum distance check
                {
                    Vector3 targetPos = new Vector3(groundDirection.magnitude, direction.y, 0);

                    float height = targetPos.y + targetPos.magnitude / 2f;
                    height = Mathf.Max(0.01f, height);

                    float angle, v0, time;
                    CalculatePathWithH(targetPos, height, out angle, out v0, out time);
                    DrawPath(groundDirection.normalized, v0, angle, time, _Step);
                    //FindLandingPoint(_Line.positionCount.);
                }
            }
        }
        else if (_Line != null && !Input.GetKey(KeyCode.J))
        {
            _Line.enabled = false;
        }

        // RELEASE J: Throw the ember
        if (Input.GetKeyUp(KeyCode.J))
        {
            Debug.Log($"<color=cyan>J Key Released!</color>");
            Debug.Log($"  - Current Held Ember: {(currentHeldEmber == null ? "NULL" : currentHeldEmber.name)}");
            Debug.Log($"  - isHoldingJ: {isHoldingJ}");

            if (currentHeldEmber != null && isHoldingJ)
            {
                Debug.Log($"<color=cyan>Processing throw for {currentHeldEmber.name}</color>");

                Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000f))
                {
                    Vector3 direction = hit.point - Spoon.position;
                    Vector3 groundDirection = new Vector3(direction.x, 0, direction.z);

                    Debug.Log($"  - Hit point: {hit.point}");
                    Debug.Log($"  - Direction magnitude: {groundDirection.magnitude}");

                    if (groundDirection.magnitude > 0.1f)
                    {
                        Vector3 targetPos = new Vector3(groundDirection.magnitude, direction.y, 0);

                        float height = targetPos.y + targetPos.magnitude / 2f;
                        height = Mathf.Max(0.01f, height);

                        float angle, v0, time;
                        CalculatePathWithH(targetPos, height, out angle, out v0, out time);

                        GameObject emberBeingThrown = currentHeldEmber;

                        // Clear references BEFORE throwing (allows consecutive throws)
                        currentHeldEmber = null;
                        isHoldingJ = false;

                        _Throw();

                        // FIXED: Don't stop coroutines - allow multiple simultaneous throws
                        // Each ember has its own coroutine
                        StartCoroutine(Courotine_Movement(groundDirection.normalized, v0, angle, time, emberBeingThrown));

                        Debug.Log($"<color=green>✓ Threw {emberBeingThrown.name}!</color>");
                        Debug.Log($"  - Direction: {groundDirection.normalized}");
                        Debug.Log($"  - Velocity: {v0}");
                        Debug.Log($"  - Time: {time}");
                    }
                    else
                    {
                        Debug.LogWarning("Target too close to throw!");
                    }
                }
                else
                {
                    Debug.LogWarning("Raycast didn't hit anything!");
                }

                if (_Line != null) _Line.enabled = false;
            }
        }
    }

    private void DrawPath(Vector3 direction, float v0, float angle, float time, float step)
    {
        if (_Line == null) return;

        step = Mathf.Max(0.01f, step);
        _Line.positionCount = (int)(time / step) + 2;
        int count = 0;

        for (float i = 0; i < time; i += step)
        {
            float x = v0 * i * Mathf.Cos(angle);
            float y = v0 * i * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(i, 2);
            _Line.SetPosition(count, Spoon.position + direction * x + Vector3.up * y);
            count++;
        }

        float xfinal = v0 * time * Mathf.Cos(angle);
        float yfinal = v0 * time * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(time, 2);
        _Line.SetPosition(count, Spoon.position + direction * xfinal + Vector3.up * yfinal);
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

    IEnumerator Courotine_Movement(Vector3 direction, float v0, float angle, float time, GameObject ember)
    {
        Debug.Log($"<color=magenta>Starting throw coroutine for {ember.name}</color>");

        // FIXED: Store the starting position so ember doesn't follow player
        Vector3 throwStartPosition = Spoon.position;

        // Disable collider during flight to prevent mid-air collisions
        Collider emberCollider = ember.GetComponent<Collider>();
        if (emberCollider != null)
        {
            emberCollider.enabled = false;
        }

        float t = 0;

        while (t < time && ember != null)
        {
            float x = v0 * t * Mathf.Cos(angle);
            float y = v0 * t * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(t, 2);

            // Use stored start position instead of current Spoon position
            ember.transform.position = throwStartPosition + direction * x + Vector3.up * y;
            t += Time.deltaTime;
            yield return null;
        }

        if (ember == null)
        {
            Debug.LogWarning("Ember was destroyed during throw!");
            yield break;
        }

        // Re-enable collider when landed
        if (emberCollider != null)
        {
            emberCollider.enabled = true;
        }

        // Re-enable NavMeshAgent when landed
        NavMeshAgent agent = ember.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
        }

        // Release ember back to FREE state after landing
        EmberStateManager.Instance.ReleaseEmber(ember);
        IsThrown = false;
        Debug.Log($"<color=green>✓ {ember.name} landed at {ember.transform.position}</color>");
    }

    public void _Holding(GameObject Ember)
    {
        IsThrown = false;
        Ember.transform.SetParent(Spoon);
        Ember.transform.localPosition = Vector3.zero; // Center on spoon

        // Disable NavMeshAgent while being held
        NavMeshAgent agent = Ember.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            Debug.Log($"  - Disabled NavMeshAgent");
        }

        Rigidbody rb = Ember.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Debug.Log($"  - Set Rigidbody to kinematic");
        }

        // FIXED: Disable collider during hold to prevent physics issues
        Collider col = Ember.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
    private void FindLandingPoint(Vector3[] trajectoryPoints)
    {
        // Find the first point that hits the ground
        for (int i = 1; i < trajectoryPoints.Length; i++)
        {
            Vector3 point = trajectoryPoints[i];

            // Raycast down to find ground
            RaycastHit hit;
            if (Physics.Raycast(point + Vector3.up * 2f, Vector3.down, out hit, 10f, groundLayer))
            {
                predictedLandingPoint = hit.point;

                // Check if it's on NavMesh
                NavMeshHit navHit;
                validLandingSpot = NavMesh.SamplePosition(predictedLandingPoint, out navHit, groundCheckRadius, NavMesh.AllAreas);

                if (validLandingSpot)
                {
                    predictedLandingPoint = navHit.position;
                }

                // Show landing indicator
                if (landingIndicator != null)
                {
                    landingIndicator.SetActive(true);
                    landingIndicator.transform.position = predictedLandingPoint + Vector3.up * indicatorHoverHeight;

                    // Change color based on validity
                    Renderer rend = landingIndicator.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        Color indicatorColor = validLandingSpot ? Color.green : Color.red;
                        rend.material.SetColor("_Color", indicatorColor);
                        if (rend.material.HasProperty("_EmissionColor"))
                        {
                            rend.material.SetColor("_EmissionColor", indicatorColor);
                        }
                    }
                }

                return;
            }
        }

        // No landing point found
        validLandingSpot = false;
        if (landingIndicator != null)
        {
            landingIndicator.SetActive(false);
        }
    }

    public void _Throw()
    {
        IsThrown = true;
        if (Spoon.childCount > 0)
        {
            GameObject Ember = Spoon.GetChild(0).gameObject;
            Ember.transform.parent = null;

            Debug.Log($"  - Unparented {Ember.name} from Spoon");

            // FIXED: Re-enable collider immediately on throw
            Collider col = Ember.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }

            // Keep NavMeshAgent disabled during flight
            // It will be re-enabled when it lands

            Rigidbody rb = Ember.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Keep kinematic during scripted flight
                Debug.Log($"  - Kept Rigidbody kinematic for flight");
            }
            Spoon.DetachChildren();
        }
    }
}