using UnityEngine;
using UnityEngine.AI;
public class GarbonzoBean : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float hoverHeight = 0.5f;
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float carrySpeed = 6f; // Speed when being carried
    [SerializeField] private GameObject pot;

    private EmberBehavior carrierEmber;
    private bool isBeingCarried = false;
    private bool hasCarrier = false; // Lock to prevent multiple embers
    private Rigidbody rb;
    private Vector3 spawnPosition; // Remember where this Garbonzo spawned

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Start as kinematic to prevent falling
            rb.useGravity = false; // No gravity initially
        }

        // Remember spawn position for ember to return to
        spawnPosition = transform.position;
    }

    private void Start()
    {
        if (pot == null)
        {
            pot = GameObject.FindGameObjectWithTag("Pot");
        }

        Debug.Log($"<color=orange>Garbonzo spawned at {spawnPosition}</color>");
    }

    private void Update()
    {
        if (!isBeingCarried && !hasCarrier)
        {
            DetectNearbyEmber();
        }
        else if (isBeingCarried && carrierEmber != null)
        {
            // Check if ember is still in correct state
            EmberState emberState = EmberStateManager.Instance.GetEmberState(carrierEmber.gameObject);
            if (emberState != EmberState.ON_GARBANZO_BEANS)
            {
                Debug.Log($"<color=yellow>Ember state changed to {emberState}, dropping Garbonzo!</color>");
                Drop();
                return;
            }

            UpdateCarrying();
        }
    }

    private void DetectNearbyEmber()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Ember"))
            {
                EmberBehavior ember = col.GetComponent<EmberBehavior>();
                if (ember != null)
                {
                    // Check if ember is FREE
                    EmberState state = EmberStateManager.Instance.GetEmberState(col.gameObject);
                    if (state == EmberState.FREE)
                    {
                        AttachToEmber(ember);
                        break;
                    }
                }
            }
        }
    }

    private void AttachToEmber(EmberBehavior ember)
    {
        if (hasCarrier)
        {
            return;
        }

        // Claim the ember with new ON_GARBONZO_BEANS state
        if (EmberStateManager.Instance.TryClaimEmber(ember.gameObject, EmberState.ON_GARBANZO_BEANS))
        {
            carrierEmber = ember;
            isBeingCarried = true;
            hasCarrier = true;

            // Disable physics
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // Disable collider
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.enabled = false;
            }

            Debug.Log($"<color=orange>{gameObject.name} picked up by {ember.gameObject.name}</color>");

            // Start moving to pot
            StartCarrying();
        }
    }

    private void StartCarrying()
    {
        if (pot != null && carrierEmber != null)
        {
            NavMeshAgent agent = carrierEmber.GetAgent();
            if (agent != null)
            {
                // Ensure agent is enabled and on NavMesh
                if (!agent.enabled)
                {
                    agent.enabled = true;
                }

                if (!agent.isOnNavMesh)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(carrierEmber.transform.position, out hit, 2f, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                    }
                }

                // CRITICAL: Configure agent for movement
                agent.speed = carrySpeed;
                agent.acceleration = 8f;
                agent.angularSpeed = 120f;
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.updatePosition = true;
                agent.SetDestination(pot.transform.position);

                Debug.Log($"<color=orange>Garbonzo being carried to pot at speed {carrySpeed}</color>");
            }
        }
    }

    private void UpdateCarrying()
    {
        // Check if ember still exists and is still carrying us
        if (carrierEmber == null)
        {
            Drop();
            return;
        }

        // Check ember state - if not ON_GARBONZO_BEANS, drop
        EmberState emberState = EmberStateManager.Instance.GetEmberState(carrierEmber.gameObject);
        if (emberState != EmberState.ON_GARBANZO_BEANS)
        {
            Debug.Log($"<color=yellow>Ember changed state to {emberState}, dropping Garbonzo</color>");
            Drop();
            return;
        }

        // Keep updating the agent's destination every frame
        NavMeshAgent agent = carrierEmber.GetAgent();
        if (agent != null && pot != null)
        {
            // Ensure agent is still configured correctly
            if (agent.isStopped)
            {
                Debug.LogWarning($"<color=yellow>Garbonzo agent was stopped! Re-enabling...</color>");
                agent.isStopped = false;
            }

            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                agent.SetDestination(pot.transform.position);
            }

            // Debug every 60 frames
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"<color=orange>{gameObject.name} carrying status:</color>");
                Debug.Log($"  Agent Speed: {agent.speed}");
                Debug.Log($"  Agent isStopped: {agent.isStopped}");
                Debug.Log($"  Agent hasPath: {agent.hasPath}");
                Debug.Log($"  Agent pathStatus: {agent.pathStatus}");
                Debug.Log($"  Agent velocity: {agent.velocity.magnitude:F2}");
                Debug.Log($"  Distance to pot: {Vector3.Distance(transform.position, pot.transform.position):F2}");
            }
        }

        // Position Garbozo above ember
        Vector3 targetPosition = carrierEmber.transform.position + Vector3.up * hoverHeight;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

        // Check if reached pot
        if (pot != null)
        {
            float distanceToPot = Vector3.Distance(transform.position, pot.transform.position);
            if (distanceToPot < 2f)
            {
                DeliverToPot();
            }
        }
    }

    private void Drop()
    {
        isBeingCarried = false;

        // Release ember before clearing reference
        if (carrierEmber != null)
        {
            EmberStateManager.Instance.ReleaseEmber(carrierEmber.gameObject);
            carrierEmber = null;
        }

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = true;
            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, transform.position.z);
        }

        // Re-enable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        Debug.Log($"<color=yellow>Charcoal dropped</color>");
    }

    private void DeliverToPot()
    {
        Debug.Log($"<color=green>✓ Garbonzo delivered to pot!</color>");

        // Store ember reference and spawn position before destroying charcoal
        EmberBehavior deliveryEmber = carrierEmber;
        Vector3 returnPosition = spawnPosition;

        // Spawn new ember at pot
        PotController potController = pot.GetComponent<PotController>();
        if (potController != null)
        {
            potController.SpawnNewEmber(pot.transform.position);
        }

        // Release carrier ember
        if (deliveryEmber != null)
        {
            EmberStateManager.Instance.ReleaseEmber(deliveryEmber.gameObject);

            // Check if there are more charcoal pieces at the spawn location
            Collider[] nearbyColl = Physics.OverlapSphere(returnPosition, 5f);
            bool hasMoreGarbonzo = false;
            foreach (Collider col in nearbyColl)
            {
                GarbonzoBean Bean = col.GetComponent<GarbonzoBean>();
                if (Bean != null && Bean != this && !Bean.HasCarrier())
                {
                    hasMoreGarbonzo = true;
                    break;
                }
            }

            // Only send ember back if there's more charcoal
            if (hasMoreGarbonzo)
            {
                NavMeshAgent agent = deliveryEmber.GetAgent();
                if (agent != null)
                {
                    agent.speed = 3.5f; // Normal speed for return trip
                    agent.isStopped = false;
                    agent.SetDestination(returnPosition);
                    Debug.Log($"<color=magenta>Sending ember back to charcoal pile at {returnPosition} (more charcoal available)</color>");
                }
            }
            else
            {
                Debug.Log($"<color=grey>No more charcoal at pile, ember stays at pot</color>");
            }
        }

        // Destroy charcoal
        Destroy(gameObject);
    }

    public bool HasCarrier()
    {
        return hasCarrier;
    }

    private void OnDrawGizmosSelected()
    {
        // Show detection radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (isBeingCarried && pot != null)
        {
            // Show line to pot
            Gizmos.color = Color.orange;
            Gizmos.DrawLine(transform.position, pot.transform.position);
        }

        // Show spawn position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnPosition, 0.2f);
    }


    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }

  

}
