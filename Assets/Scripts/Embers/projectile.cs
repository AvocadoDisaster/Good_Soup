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
        //currentHeldEmber = this.gameObject;
}

private void Update()
    {
        
        
        // PRESS J: Pick up nearest ingredient
        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("<color=yellow>J Key Pressed Down</color>");

            if (currentHeldEmber == null)
            {
                GameObject nearest = FindNearestEmber();
                if (nearest != null)
                {
                    
                    currentHeldEmber = nearest;
                    _Holding(currentHeldEmber);
                    isHoldingJ = true;
                    hasLockedDirection = false;

                    Debug.Log($"<color=green>Picked up {currentHeldEmber.name}</color>");
                }
                else
                {
                    Debug.Log("<color=red>No ember or ingredient nearby!</color>");
                }
            }
        }

        // HOLD J: Show trajectory and lock direction
        if (Input.GetKey(KeyCode.J) && currentHeldEmber != null)
        {
            if (_Line != null) _Line.enabled = true;

            Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            int layerMask = ~(LayerMask.GetMask("Ingredient") | LayerMask.GetMask("Ember"));
            if (groundLayer.value != 0)
            {
                layerMask = groundLayer.value;
            }

            if (Physics.Raycast(ray, out hit, 500f, layerMask))
            {
                Vector3 direction = hit.point - Spoon.position;
                Vector3 groundDirection = new Vector3(direction.x, 0, direction.z);

                if (groundDirection.magnitude > 0.1f)
                {
                    if (!hasLockedDirection)
                    {
                        lockedThrowDirection = groundDirection.normalized;
                        hasLockedDirection = true;
                    }

                    Vector3 targetPos = new Vector3(groundDirection.magnitude, direction.y, 0);
                    float height = targetPos.y + targetPos.magnitude / 2f;
                    height = Mathf.Max(0.01f, height);

                    float angle, v0, time;
                    CalculatePathWithH(targetPos, height, out angle, out v0, out time);
                    DrawPath(lockedThrowDirection, v0, angle, time, _Step);
                }
            }
        }
        else if (_Line != null && !Input.GetKey(KeyCode.J))
        {
            _Line.enabled = false;
        }

        // RELEASE J: Throw ingredient
        if (Input.GetKeyUp(KeyCode.J))
        {
            if (currentHeldEmber != null && isHoldingJ && hasLockedDirection)
            {
                Debug.Log($"<color=cyan>Processing throw for {currentHeldEmber.name}</color>");

                Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                int layerMask = ~(LayerMask.GetMask("Ingredient") | LayerMask.GetMask("Ember"));
                if (groundLayer.value != 0)
                {
                    layerMask = groundLayer.value;
                }

                if (Physics.Raycast(ray, out hit, 1000f, layerMask))
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
                        Vector3 throwStartPos = Spoon.position;
                        Vector3 throwDirection = lockedThrowDirection;

                        // Clear player state
                        currentHeldEmber = null;
                        isHoldingJ = false;
                        hasLockedDirection = false;

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

                        Debug.Log($"<color=green> Throwing {ingredientToThrow.name}!</color>");
                        Debug.Log($"  From: {throwStartPos}, Dir: {throwDirection}");

                        // IMPORTANT: Always create a NEW ThrowController for each throw
                        // Check if there's an old one and destroy it first
                        ThrowController oldController = ingredientToThrow.GetComponent<ThrowController>();
                        if (oldController != null)
                        {
                            Debug.LogWarning($"Found existing ThrowController on {ingredientToThrow.name}, destroying it!");
                            Destroy(oldController);
                        }

                        // Add fresh controller
                        ThrowController controller = ingredientToThrow.AddComponent<ThrowController>();

                        // Wait one frame to ensure component is fully initialized
                        StartCoroutine(StartThrowNextFrame(controller, throwStartPos, throwDirection, v0, angle, time, ingredientLifetime));

                        // Spawn next ingredient
                        if (spawner != null)
                        {
                            spawner.SpawnIngredient();
                        }

                        IsThrown = true;
                    }
                    else
                    {
                        Debug.LogWarning("Target too close to throw!");
                        hasLockedDirection = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Raycast didn't hit anything!");
                    hasLockedDirection = false;
                }

                if (_Line != null) _Line.enabled = false;
            }
        }
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

            ThrowController tc = col.GetComponent<ThrowController>();
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

    private void DrawPath(Vector3 direction, float v0, float angle, float time, float step)
    {
        if (_Line == null) return;

        Vector3 origin = currentHeldEmber != null ?
    currentHeldEmber.transform.position + lineOriginOffset :
    Spoon.position;


        step = Mathf.Max(0.01f, step);
        _Line.positionCount = (int)(time / step) + 2;
        int count = 0;

        for (float i = 0; i < time; i += step)
        {
            float x = v0 * i * Mathf.Cos(angle);
            float y = v0 * i * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(i, 2);
            _Line.SetPosition(count, origin + direction * x + Vector3.up * y);
            count++;
        }

        float xf = v0 * time * Mathf.Cos(angle);
        float yf = v0 * time * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(time, 2);
        _Line.SetPosition(count, origin + direction * xf + Vector3.up * yf);
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

    // Helper coroutine to ensure component initialization
    private IEnumerator StartThrowNextFrame(ThrowController controller, Vector3 startPos, Vector3 direction, float v0, float angle, float time, float lifetime)
    {
        yield return null; // Wait one frame

        if (controller != null)
        {
            controller.StartThrow(startPos, direction, v0, angle, time, lifetime);
        }
        else
        {
            Debug.LogError("ThrowController was destroyed before throw could start!");
        }
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

        Ember.transform.SetParent(Spoon);
        Ember.transform.localPosition = Vector3.zero; // FIXED

        Rigidbody emberRb = Ember.GetComponent<Rigidbody>();
        if (emberRb != null)
        {
            emberRb.isKinematic = true;
            emberRb.useGravity = false;
            emberRb.linearVelocity = Vector3.zero;
            emberRb.angularVelocity = Vector3.zero;
        }

        Collider emberCol = Ember.GetComponent<Collider>();
        if (emberCol != null) emberCol.enabled = false;
    }


    internal void SetSpoon(Transform spoon)
    {
        Spoon = spoon.transform;
    }
}