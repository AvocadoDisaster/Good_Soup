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
        if (collision.CompareTag("Ember"))
        {
            GameObject ember = collision.gameObject;

            Debug.Log($"<color=cyan>========== RALLY ATTEMPT ==========</color>");
            Debug.Log($"  Ember: {ember.name}");

            // Check if ember is already in the party list
            if (inParty.InCurrentParty.Contains(ember))
            {
                Debug.Log($"<color=yellow>  Already in party!</color>");
                return;
            }

            // Get current state
            EmberState checkState = EmberStateManager.Instance.GetEmberState(ember);
            Debug.Log($"  Current State: {checkState}");

            // CRITICAL: Force drop any items before rallying
            ForceDropCarriedItems(ember, checkState);

            // If ember is on an ingredient, remove it first
            if (checkState == EmberState.ON_INGREDIENT)
            {
                Debug.Log($"<color=yellow>  Detaching from ingredient...</color>");
                EmberBehavior emberBehavior = ember.GetComponent<EmberBehavior>();
                if (emberBehavior != null)
                {
                    emberBehavior.DetachFromIngredient();
                }
            }

            // Now try to claim for rally
            if (EmberStateManager.Instance.TryClaimEmber(ember, EmberState.IN_RALLY_PARTY))
            {
                // Successfully claimed! Add to party
                inParty.InCurrentParty.Add(ember);

                // Setup NavMeshAgent
                NavMeshAgent emberAgent = ember.GetComponent<NavMeshAgent>();
                if (emberAgent == null)
                {
                    Debug.LogError($"<color=red>  No NavMeshAgent!</color>");
                    return;
                }

                if (!emberAgent.enabled)
                {
                    emberAgent.enabled = true;
                }

                if (!emberAgent.isOnNavMesh)
                {
                    Debug.LogError($"<color=red>  NOT on NavMesh at {ember.transform.position}</color>");
                    return;
                }

                // Configure agent
                emberAgent.isStopped = false;
                emberAgent.updateRotation = true;
                emberAgent.updatePosition = true;

                // Make sure Rigidbody isn't blocking movement
                Rigidbody rb = ember.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                emberAgent.SetDestination(Spoon.position);

                Debug.Log($"<color=green>✓ {ember.name} RALLIED! Total in party: {inParty.InCurrentParty.Count}</color>");
            }
            else
            {
                EmberState finalState = EmberStateManager.Instance.GetEmberState(ember);
                Debug.Log($"<color=red>❌ Cannot rally - state: {finalState}</color>");
            }
        }
    }

    private void ForceDropCarriedItems(GameObject ember, EmberState state)
    {
        Debug.Log($"<color=yellow>  Checking for carried items...</color>");

        // Check for charcoal (using new Unity API)
        Charcoal[] allCharcoal = FindObjectsByType<Charcoal>(FindObjectsSortMode.None);
        foreach (Charcoal charcoal in allCharcoal)
        {
            // Use reflection to get private carrierEmber field
            var field = typeof(Charcoal).GetField("carrierEmber",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                EmberBehavior carrierEmber = field.GetValue(charcoal) as EmberBehavior;
                if (carrierEmber != null && carrierEmber.gameObject == ember)
                {
                    Debug.Log($"<color=orange>  Found charcoal carried by this ember - forcing drop!</color>");
                    // Call Drop method using reflection
                    var dropMethod = typeof(Charcoal).GetMethod("Drop",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    dropMethod?.Invoke(charcoal, null);
                }
            }
        }

        // Check for garbanzo beans (using new Unity API)
        GarbanzoBean[] allBeans = FindObjectsByType<GarbanzoBean>(FindObjectsSortMode.None);
        foreach (GarbanzoBean bean in allBeans)
        {
            var field = typeof(GarbanzoBean).GetField("carrierEmber",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                EmberBehavior carrierEmber = field.GetValue(bean) as EmberBehavior;
                if (carrierEmber != null && carrierEmber.gameObject == ember)
                {
                    Debug.Log($"<color=cyan>  Found bean carried by this ember - forcing drop!</color>");
                    var dropMethod = typeof(GarbanzoBean).GetMethod("Drop",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    dropMethod?.Invoke(bean, null);
                }
            }
        }
    }

    public void OnTriggerStay(Collider collision)
    {
        if (collision.CompareTag("Ember"))
        {
            GameObject ember = collision.gameObject;

            if (inParty.InCurrentParty.Contains(ember))
            {
                NavMeshAgent emberAgent = ember.GetComponent<NavMeshAgent>();
                if (emberAgent != null && emberAgent.enabled && emberAgent.isOnNavMesh)
                {
                    if (Vector3.Distance(emberAgent.destination, Spoon.position) > 0.5f)
                    {
                        emberAgent.SetDestination(Spoon.position);
                    }
                }
            }
        }
    }

    private void UpdateEmberDestinations()
    {
        if (Spoon == null) return;

        foreach (GameObject ember in inParty.InCurrentParty)
        {
            if (ember == null) continue;

            NavMeshAgent agent = ember.GetComponent<NavMeshAgent>();
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                if (Vector3.Distance(agent.destination, Spoon.position) > 1f)
                {
                    agent.SetDestination(Spoon.position);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && Spoon != null && inParty != null)
        {
            Gizmos.color = Color.cyan;
            foreach (GameObject ember in inParty.InCurrentParty)
            {
                if (ember != null)
                {
                    Gizmos.DrawLine(ember.transform.position, Spoon.position);
                }
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Spoon.position, 0.5f);
        }
    }
}