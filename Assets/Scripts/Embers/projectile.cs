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
   
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.1f;
    
    [SerializeField] private float indicatorHoverHeight = 0.2f;
    public GameObject currentHeldEmber = null;
    private bool isHoldingJ = false;
    
    [SerializeField] private float groundCheckRadius = 1f;
   
    

    private void Start()
    {
        _Camera = Camera.main;
        if (_Line != null)
        {
            _Line.enabled = false;
        }
        
    }
    

    private void Update()
    {




        // PRESS J: Try to pick up the nearest ember/ingredient
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("<color=yellow>J Key Pressed Down!</color>");

            // If not already holding something
            if (currentHeldEmber == null)
            {
                GameObject nearest = FindNearestEmber();
                if (nearest != null)
                {
                    currentHeldEmber = nearest;
                    _Holding(currentHeldEmber);
                    isHoldingJ = true;

                    Debug.Log($"<color=green>✓ Picked up {currentHeldEmber.name}</color>");
                }
                else
                {
                    Debug.Log("<color=red>No ember or ingredient nearby!</color>");
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

                        
                        currentHeldEmber = null;
                        isHoldingJ = false;

                        _Throw();

                        
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
    private GameObject FindNearestEmber()
    {
        float detectRadius = 2.5f; // how close the ember must be to pick up
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, detectRadius);

        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider col in nearbyObjects)
        {
            // Check tag or component type for embers/ingredients
            if (col.CompareTag("Ember") || col.CompareTag("Ingredient"))
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = col.gameObject;
                }
            }
        }

        return nearest;
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
        // Release ember back to FREE state after landing
        
        IsThrown = false;
        
    }

    public void _Holding(GameObject Ember)
    {
        IsThrown = false;
        Ember.transform.SetParent(Spoon);
        Ember.transform.localPosition = Vector3.zero; // Center on spoon

        // Disable NavMeshAgent while being held
       

        Rigidbody rb = Ember.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Debug.Log($"  - Set Rigidbody to kinematic");
        }

        // Disable collider during hold to prevent physics issues
        Collider col = Ember.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
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

            
            Collider col = Ember.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = true;
            }

           

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