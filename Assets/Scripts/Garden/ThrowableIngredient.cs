using UnityEngine;



[RequireComponent(typeof(Rigidbody))]
public class ThrowableIngredient : MonoBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float maxThrowForce = 25f;
    [SerializeField] private float minThrowForce = 5f;

    [Header("State")]
    [SerializeField] private bool isHeld = false;
    [SerializeField] private bool hasBeenThrown = false;

    private Rigidbody rb;
    private Collider col;
    private Vector3 throwDirection;
    private float currentThrowForce;

    private void Update()
    {
        PickUp();
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Start with physics disabled (picked up state)
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void PickUp()
    {
        isHeld = true;
        hasBeenThrown = false;

        // Disable physics
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"<color=green>Picked up: {gameObject.name}</color>");
    }

    public void Throw(Vector3 direction, float force)
    {
        if (hasBeenThrown) return;

        throwDirection = direction.normalized;
        currentThrowForce = Mathf.Clamp(force, minThrowForce, maxThrowForce);

        isHeld = false;
        hasBeenThrown = true;

        // Enable physics
        rb.isKinematic = false;
        rb.useGravity = true;

        // Apply throw force
        rb.AddForce(throwDirection * currentThrowForce, ForceMode.VelocityChange);

        // Add slight spin for realism
        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.VelocityChange);

        Debug.Log($"<color=yellow>Threw {gameObject.name} with force {currentThrowForce}</color>");
    }

    public bool IsHeld()
    {
        return isHeld;
    }

    public bool HasBeenThrown()
    {
        return hasBeenThrown;
    }

    public void ResetIngredient()
    {
        hasBeenThrown = false;
        isHeld = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenThrown && other.CompareTag("Pot"))
        {
            Debug.Log($"<color=green> {gameObject.name} HIT THE POT!</color>");

            // Notify game manager
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.IngredientScored(gameObject.name);
            }

            // Destroy ingredient
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (isHeld)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else if (hasBeenThrown)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}