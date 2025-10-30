using UnityEngine;
using UnityEngine.AI;

public class PlayerRallyController : MonoBehaviour
{
    public float rallyRange = 12f;
    public float rallyAngle = 50f;
    public Transform rallyOrigin;
    public ParticleSystem soundWaveFX;
    [SerializeField] public InParty inParty;
    [SerializeField] private Transform Spoon;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EmitRallyCone();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            transform.localScale = Vector3.zero;
        }

        // ALWAYS update ember destinations every frame while they're in the party
        UpdateEmberDestinations();
    }

    void EmitRallyCone()
    {
        // soundWaveFX?.Play();
        transform.localScale = new Vector3(9, 4.6f, 4);
    }

    public void OnTriggerEnter(Collider collision)
    {
        // Use collision.gameObject directly instead of finding by tag
        if (collision.CompareTag("Ember"))
        {
            GameObject ember = collision.gameObject;

            Debug.Log($"<color=cyan>Rally Caller detected: {ember.name}</color>");

            // Check if ember is already in the party list
            if (inParty.InCurrentParty.Contains(ember))
            {
                Debug.Log($"<color=yellow>Ember {ember.name} already in party!</color>");
                return;
            }

            // Get current state
            EmberState checkState = EmberStateManager.Instance.GetEmberState(ember);
            Debug.Log($"<color=cyan>Rally attempt on {ember.name} - Current state: {checkState}</color>");

            // If ember is on an ingredient, remove it from ingredient first
            if (checkState == EmberState.ON_INGREDIENT)
            {
                Debug.Log($"<color=yellow>Ember {ember.name} is ON_INGREDIENT - removing from ingredient</color>");

                // Find ingredient by checking the ember's currentIngredient reference
                EmberBehavior emberBehavior = ember.GetComponent<EmberBehavior>();
                if (emberBehavior != null)
                {
                    // This will call RemoveEmber on the ingredient and set state to FREE
                    emberBehavior.DetachFromIngredient();
                    Debug.Log($"  - Detached {ember.name} from ingredient via EmberBehavior");
                }
                else
                {
                    Debug.LogWarning($"  - {ember.name} has no EmberBehavior component!");
                }
            }

            // Now try to claim for rally (should be FREE after detaching)
            if (EmberStateManager.Instance.TryClaimEmber(ember, EmberState.IN_RALLY_PARTY))
            {
                // Successfully claimed! Add to party
                inParty.InCurrentParty.Add(ember);

                // DEBUG: Check NavMeshAgent
                NavMeshAgent emberAgent = ember.GetComponent<NavMeshAgent>();
                if (emberAgent == null)
                {
                    Debug.LogError($" {ember.name} has NO NavMeshAgent component!");
                    return;
                }

                if (!emberAgent.enabled)
                {
                    Debug.LogWarning($" {ember.name} NavMeshAgent is DISABLED. Enabling...");
                    emberAgent.enabled = true;
                }

                if (!emberAgent.isOnNavMesh)
                {
                    Debug.LogError($" {ember.name} is NOT on NavMesh! Position: {ember.transform.position}");
                    return;
                }

                if (Spoon == null)
                {
                    Debug.LogError($" Spoon Transform is NULL!");
                    return;
                }

                // Set ember's destination to the Spoon
                emberAgent.isStopped = false;
                emberAgent.updateRotation = true;
                emberAgent.updatePosition = true;

                // Make sure Rigidbody isn't blocking movement
                Rigidbody rb = ember.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true; // NavMeshAgent controls movement
                    Debug.Log($"  - Set Rigidbody to kinematic");
                }

                emberAgent.SetDestination(Spoon.position);

                Debug.Log($"<color=green>✓ {ember.name} → Rally Party! Moving to Spoon at {Spoon.position}</color>");
                Debug.Log($"  - Agent Speed: {emberAgent.speed}");
                Debug.Log($"  - Agent Stopping Distance: {emberAgent.stoppingDistance}");
                Debug.Log($"  - Agent isStopped: {emberAgent.isStopped}");
                Debug.Log($"  - Agent hasPath: {emberAgent.hasPath}");
                Debug.Log($"  - Agent pathPending: {emberAgent.pathPending}");
                Debug.Log($"  - Distance to Spoon: {Vector3.Distance(ember.transform.position, Spoon.position):F2}");
                Debug.Log($"  - Total in party: {inParty.InCurrentParty.Count}");
            }
            else
            {
                // Ember is already claimed by another system
                EmberState finalState = EmberStateManager.Instance.GetEmberState(ember);
                Debug.Log($"<color=red>Cannot rally {ember.name} - currently in state: {finalState}</color>");
            }
        }
    }

    // Optional: If you want embers to continuously update their destination
    public void OnTriggerStay(Collider collision)
    {
        if (collision.CompareTag("Ember"))
        {
            GameObject ember = collision.gameObject;

            // Only update destination if ember is in our party
            if (inParty.InCurrentParty.Contains(ember))
            {
                NavMeshAgent emberAgent = ember.GetComponent<NavMeshAgent>();
                if (emberAgent != null && emberAgent.enabled && emberAgent.isOnNavMesh)
                {
                    // Keep updating destination in case Spoon moves
                    if (Vector3.Distance(emberAgent.destination, Spoon.position) > 0.5f)
                    {
                        emberAgent.SetDestination(Spoon.position);
                    }
                }
            }
        }
    }

    // Continuously update destinations for all party members
    private void UpdateEmberDestinations()
    {
        if (Spoon == null) return;

        foreach (GameObject ember in inParty.InCurrentParty)
        {
            if (ember == null) continue;

            NavMeshAgent agent = ember.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                // Only update if destination has changed significantly
                if (Vector3.Distance(agent.destination, Spoon.position) > 1f)
                {
                    agent.SetDestination(Spoon.position);
                }
            }
        }
    }

    // Optional: Visual debug
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && Spoon != null && inParty != null)
        {
            // Draw lines from embers to spoon
            Gizmos.color = Color.cyan;
            foreach (GameObject ember in inParty.InCurrentParty)
            {
                if (ember != null)
                {
                    Gizmos.DrawLine(ember.transform.position, Spoon.position);
                }
            }

            // Draw spoon position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Spoon.position, 0.5f);
        }
    }
}