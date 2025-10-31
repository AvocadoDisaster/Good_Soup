using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class EmberThrowable : MonoBehaviour
{
    [Header("Landing Settings")]
    [SerializeField] private float groundSnapSpeed = 20f;
    [SerializeField] private float maxGroundSearchDistance = 10f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private bool isThrown = false;
    private bool isLanding = false;
    private Vector3 targetLandingPoint;
    private Vector3 groundPoint;
    private bool hasFoundGround = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void Throw(Vector3 velocity, Vector3 predictedLanding)
    {
        Debug.Log($"<color=yellow>=== EMBER THROW START ===</color>");
        Debug.Log($"  Ember: {gameObject.name}");
        Debug.Log($"  Velocity: {velocity}");
        Debug.Log($"  Predicted Landing: {predictedLanding}");

        isThrown = true;
        isLanding = false;
        hasFoundGround = false;
        targetLandingPoint = predictedLanding;

        // Disable NavMeshAgent during throw
        if (agent != null)
        {
            agent.enabled = false;
            Debug.Log($"  NavMeshAgent disabled for throw");
        }

        // Enable Rigidbody physics
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = velocity;
            Debug.Log($"  Rigidbody enabled with velocity: {velocity}");
        }

        Debug.Log($"<color=yellow>Ember is now in flight!</color>");
    }

    private void FixedUpdate()
    {
        if (!isThrown) return;

        // Check if we're falling and close to predicted landing
        if (!isLanding && rb != null)
        {
            float distanceToLanding = Vector3.Distance(transform.position, targetLandingPoint);
            
            // Start landing sequence when close to predicted point or falling
            if (distanceToLanding < 2f || (rb.linearVelocity.y < 0 && distanceToLanding < 5f))
            {
                Debug.Log($"<color=cyan>Starting landing sequence at distance: {distanceToLanding:F2}</color>");
                StartLanding();
            }
        }

        // Handle landing
        if (isLanding)
        {
            UpdateLanding();
        }
    }

    private void StartLanding()
    {
        isLanding = true;

        Debug.Log($"<color=cyan>=== LANDING SEQUENCE START ===</color>");
        Debug.Log($"  Current position: {transform.position}");
        Debug.Log($"  Target landing: {targetLandingPoint}");

        // Try to find ground below us
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out hit, maxGroundSearchDistance, groundLayer))
        {
            groundPoint = hit.point;
            hasFoundGround = true;
            Debug.Log($"<color=green>  Found ground at: {groundPoint}</color>");
        }
        else
        {
            // Use predicted landing point as fallback
            groundPoint = targetLandingPoint;
            hasFoundGround = true;
            Debug.LogWarning($"<color=yellow>  No ground found by raycast, using predicted point</color>");
        }

        // Try to find NavMesh near ground point
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(groundPoint, out navHit, 5f, NavMesh.AllAreas))
        {
            groundPoint = navHit.position;
            Debug.Log($"<color=green>  Found NavMesh at: {groundPoint}</color>");
        }
        else
        {
            Debug.LogWarning($"<color=yellow>  No NavMesh found near landing point!</color>");
        }

        // Disable physics for smooth landing
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log($"  Rigidbody made kinematic");
        }
    }

    private void UpdateLanding()
    {
        if (!hasFoundGround) return;

        // Smoothly move to ground point
        float step = groundSnapSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, groundPoint, step);

        // Check if we've reached the ground
        float distanceToGround = Vector3.Distance(transform.position, groundPoint);
        
        if (distanceToGround < 0.1f)
        {
            // Snap to final position
            transform.position = groundPoint;
            CompleteLanding();
        }
    }

    private void CompleteLanding()
    {
        Debug.Log($"<color=green>=== EMBER LANDED SUCCESSFULLY ===</color>");
        Debug.Log($"  Final position: {transform.position}");

        isThrown = false;
        isLanding = false;

        // Re-enable NavMeshAgent
        if (agent != null)
        {
            agent.enabled = false; // Disable first
            
            // Warp to current position on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
                Debug.Log($"<color=green>  Warped agent to NavMesh: {hit.position}</color>");
            }
            else
            {
                Debug.LogError($"<color=red>  Cannot find NavMesh for agent at {transform.position}!</color>");
            }
            
            agent.enabled = true;
            
            if (agent.isOnNavMesh)
            {
                Debug.Log($"<color=green>✓ Agent successfully on NavMesh</color>");
            }
            else
            {
                Debug.LogError($"<color=red>❌ Agent NOT on NavMesh after landing!</color>");
            }
        }

        // Release ember state back to FREE
        if (EmberStateManager.Instance != null)
        {
            EmberStateManager.Instance.ReleaseEmber(gameObject);
            Debug.Log($"<color=green>  Ember released to FREE state</color>");
        }

        // Remove this component as it's no longer needed
        Destroy(this, 0.1f);
        
        Debug.Log($"<color=green>✓ Landing complete!</color>");
    }

    private void OnDrawGizmos()
    {
        if (isThrown)
        {
            // Show current state
            if (isLanding)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
                
                if (hasFoundGround)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, groundPoint);
                    Gizmos.DrawWireSphere(groundPoint, 0.3f);
                }
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, 0.3f);
            }

            // Show target landing point
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetLandingPoint, 0.4f);
        }
    }
}