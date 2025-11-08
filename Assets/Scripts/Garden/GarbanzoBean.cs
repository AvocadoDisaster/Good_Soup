using UnityEngine;
using UnityEngine.AI;

public class GarbanzoBean : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float hoverHeight = 0.5f;
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float carrySpeed = 6f;
    [SerializeField] private GameObject pot;

    private EmberBehavior carrierEmber;
    private bool isBeingCarried = false;
    private bool hasCarrier = false;
    private Rigidbody rb;
    private Vector3 spawnPosition;

    private void Awake()
    {
        Debug.Log($"<color=cyan>GarbanzoBean Awake: {gameObject.name}</color>");

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log($"  Rigidbody found and configured (kinematic)");
        }
        else
        {
            Debug.LogError($"<color=red>❌ {gameObject.name} has NO Rigidbody!</color>");
        }

        spawnPosition = transform.position;
    }

    private void Start()
    {
        Debug.Log($"<color=cyan>========== GarbanzoBean Start: {gameObject.name} ==========</color>");
        Debug.Log($"  Position: {transform.position}");
        Debug.Log($"  Spawn Position saved: {spawnPosition}");

        // Check collider setup
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.Log($"  Collider found: {col.GetType().Name}");
            Debug.Log($"  Collider enabled: {col.enabled}");
            Debug.Log($"  Collider is trigger: {col.isTrigger}");
        }
        else
        {
            Debug.LogError($"<color=red>❌ NO COLLIDER on {gameObject.name}!</color>");
        }

        // Check tag
        Debug.Log($"  Tag: {gameObject.tag}");

        if (pot == null)
        {
            pot = GameObject.FindGameObjectWithTag("Pot");
            if (pot != null)
            {
                Debug.Log($"  Found pot at: {pot.transform.position}");
            }
            else
            {
                Debug.LogError($"<color=red>❌ Cannot find Pot!</color>");
            }
        }
        else
        {
            Debug.Log($"  Pot already assigned: {pot.name}");
        }
    }

    private void Update()
    {
        // CRITICAL: Check state FIRST if we have a carrier
        if (carrierEmber != null)
        {
            EmberState emberState = EmberStateManager.Instance.GetEmberState(carrierEmber.gameObject);

            if (emberState == EmberState.IN_RALLY_PARTY ||
                emberState == EmberState.BEING_THROWN ||
                emberState != EmberState.ON_GARBANZO_BEANS)
            {
                Debug.Log($"<color=red>IMMEDIATE DROP! Ember state is {emberState}</color>");
                Drop();
                return;
            }
        }

        if (!isBeingCarried && !hasCarrier)
        {
            DetectNearbyEmber();
        }
        else if (isBeingCarried && carrierEmber != null)
        {
            UpdateCarrying();
        }

        // DEBUG: Press N to show this bean's status
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log($"<color=cyan>=== {gameObject.name} STATUS ===</color>");
            Debug.Log($"Has Carrier: {hasCarrier}");
            Debug.Log($"Being Carried: {isBeingCarried}");
            Debug.Log($"Carrier Ember: {(carrierEmber != null ? carrierEmber.gameObject.name : "NULL")}");
            Debug.Log($"Position: {transform.position}");
            Debug.Log($"Spawn Position: {spawnPosition}");
            if (pot != null)
            {
                Debug.Log($"Distance to Pot: {Vector3.Distance(transform.position, pot.transform.position):F2}");
            }
            if (carrierEmber != null)
            {
                EmberState state = EmberStateManager.Instance.GetEmberState(carrierEmber.gameObject);
                Debug.Log($"Carrier Ember State: {state}");
                NavMeshAgent agent = carrierEmber.GetAgent();
                if (agent != null)
                {
                    Debug.Log($"Agent Speed: {agent.speed}");
                    Debug.Log($"Agent Has Path: {agent.hasPath}");
                    Debug.Log($"Agent Velocity: {agent.velocity.magnitude:F2}");
                }
            }
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
                    EmberState state = EmberStateManager.Instance.GetEmberState(col.gameObject);

                    // Only log every 60 frames to reduce spam
                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"<color=yellow>{gameObject.name} detected ember: {col.gameObject.name} (State: {state})</color>");
                    }

                    if (state == EmberState.FREE)
                    {
                        Debug.Log($"<color=green>Ember is FREE! Attempting to attach...</color>");
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
            Debug.Log($"<color=grey>{gameObject.name} already has a carrier, ignoring ember</color>");
            return;
        }

        Debug.Log($"<color=cyan>AttachToEmber called for {gameObject.name}</color>");
        Debug.Log($"  Attempting to claim ember: {ember.gameObject.name}");

        if (EmberStateManager.Instance.TryClaimEmber(ember.gameObject, EmberState.ON_GARBANZO_BEANS))
        {
            carrierEmber = ember;
            isBeingCarried = true;
            hasCarrier = true;

            Debug.Log($"<color=green>✓ Successfully claimed ember!</color>");

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                Debug.Log($"  Physics disabled");
            }

            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.enabled = false;
                Debug.Log($"  Collider disabled");
            }

            Debug.Log($"<color=cyan>✓ {gameObject.name} picked up by {ember.gameObject.name}</color>");
            StartCarrying();
        }
        else
        {
            Debug.LogError($"<color=red>❌ FAILED to claim ember {ember.gameObject.name}!</color>");
        }
    }

    private void StartCarrying()
    {
        Debug.Log($"<color=cyan>StartCarrying for {gameObject.name}</color>");

        if (pot == null)
        {
            Debug.LogError($"<color=red>❌ Pot is NULL! Cannot start carrying!</color>");
            return;
        }

        if (carrierEmber == null)
        {
            Debug.LogError($"<color=red>❌ Carrier ember is NULL!</color>");
            return;
        }

        NavMeshAgent agent = carrierEmber.GetAgent();
        if (agent != null)
        {
            Debug.Log($"  NavMeshAgent found");

            if (!agent.enabled)
            {
                agent.enabled = true;
                Debug.Log($"  Enabled NavMeshAgent");
            }

            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning($"<color=yellow>⚠️ Agent not on NavMesh! Attempting to warp...</color>");
                NavMeshHit hit;
                if (NavMesh.SamplePosition(carrierEmber.transform.position, out hit, 2f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    Debug.Log($"  Warped to NavMesh at {hit.position}");
                }
                else
                {
                    Debug.LogError($"<color=red>❌ Cannot find NavMesh near ember!</color>");
                }
            }

            agent.speed = carrySpeed;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.updatePosition = true;

            bool pathSet = agent.SetDestination(pot.transform.position);

            Debug.Log($"  Speed: {agent.speed}");
            Debug.Log($"  isStopped: {agent.isStopped}");
            Debug.Log($"  Destination set: {pathSet}");
            Debug.Log($"  Destination: {pot.transform.position}");
            Debug.Log($"  Has Path: {agent.hasPath}");
            Debug.Log($"<color=cyan>✓ Garbanzo bean being carried to pot at speed {carrySpeed}</color>");
        }
        else
        {
            Debug.LogError($"<color=red>❌ Carrier ember has NO NavMeshAgent!</color>");
        }
    }

    private void UpdateCarrying()
    {
        if (carrierEmber == null)
        {
            Debug.LogWarning($"<color=yellow>{gameObject.name}: Carrier ember is NULL!</color>");
            Drop();
            return;
        }

        EmberState emberState = EmberStateManager.Instance.GetEmberState(carrierEmber.gameObject);
        if (emberState != EmberState.ON_GARBANZO_BEANS)
        {
            Debug.Log($"<color=yellow>{gameObject.name}: Ember changed state to {emberState}, DROPPING!</color>");
            Drop();
            return;
        }

        NavMeshAgent agent = carrierEmber.GetAgent();
        if (agent != null && pot != null)
        {
            if (agent.isStopped)
            {
                Debug.LogWarning($"<color=yellow>Agent was stopped! Re-enabling...</color>");
                agent.isStopped = false;
            }

            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                agent.SetDestination(pot.transform.position);
            }

            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"<color=cyan>{gameObject.name} carrying status:</color>");
                Debug.Log($"  Agent Speed: {agent.speed}");
                Debug.Log($"  Agent isStopped: {agent.isStopped}");
                Debug.Log($"  Agent hasPath: {agent.hasPath}");
                Debug.Log($"  Agent pathStatus: {agent.pathStatus}");
                Debug.Log($"  Agent velocity: {agent.velocity.magnitude:F2}");
                Debug.Log($"  Distance to pot: {Vector3.Distance(transform.position, pot.transform.position):F2}");
            }
        }

        Vector3 targetPosition = carrierEmber.transform.position + Vector3.up * hoverHeight;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

        if (pot != null)
        {
            float distanceToPot = Vector3.Distance(transform.position, pot.transform.position);
            if (distanceToPot < 2f)
            {
                Debug.Log($"<color=green>{gameObject.name} reached pot! Distance: {distanceToPot:F2}</color>");
                DeliverToPot();
            }
        }
    }

    private void Drop()
    {
        Debug.Log($"<color=red>=== DROP CALLED for {gameObject.name} ===</color>");
        Debug.Log($"  Current position: {transform.position}");

        EmberBehavior tempEmber = carrierEmber;
        carrierEmber = null;
        isBeingCarried = false;
        hasCarrier = false;

        if (tempEmber != null)
        {
            Debug.Log($"  Releasing ember: {tempEmber.gameObject.name}");
            EmberStateManager.Instance.ReleaseEmber(tempEmber.gameObject);
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
           // rb.linearVelocity; 
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
            Debug.Log($"  Collider re-enabled");
        }

        Debug.Log($"<color=yellow>✓ {gameObject.name} dropped at position {transform.position}</color>");
        transform.position = new Vector3(this.transform.position.x, this.transform.position.y-3f, this.transform.position.z);
    }

    private void DeliverToPot()
    {
        Debug.Log($"<color=green>=== {gameObject.name} DELIVERED TO POT ===</color>");

        EmberBehavior deliveryEmber = carrierEmber;
        Vector3 returnPosition = spawnPosition;

        // PotController will handle this via OnTriggerEnter, so we don't destroy here
        // The destruction happens in PotController.ReceiveGarbanzoBean()

        // Release carrier ember and check if there are more beans
        if (deliveryEmber != null)
        {
            Debug.Log($"  Releasing carrier ember: {deliveryEmber.gameObject.name}");
            EmberStateManager.Instance.ReleaseEmber(deliveryEmber.gameObject);

            // Check if there are more beans at the spawn location
            Collider[] nearbyColl = Physics.OverlapSphere(returnPosition, 5f);
            bool hasMoreBeans = false;
            foreach (Collider col in nearbyColl)
            {
                GarbanzoBean bean = col.GetComponent<GarbanzoBean>();
                if (bean != null && bean != this && !bean.HasCarrier())
                {
                    hasMoreBeans = true;
                    Debug.Log($"<color=cyan>  Found more beans at pile: {bean.gameObject.name}</color>");
                    break;
                }
            }

            // Only send ember back if there are more beans
            if (hasMoreBeans)
            {
                NavMeshAgent agent = deliveryEmber.GetAgent();
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.speed = 3.5f;
                    agent.isStopped = false;
                    bool pathSet = agent.SetDestination(returnPosition);
                    Debug.Log($"<color=magenta>  Sending ember back to bean pile at {returnPosition}</color>");
                    Debug.Log($"  Path set successfully: {pathSet}</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>  Cannot send ember back - agent issues</color>");
                }
            }
            else
            {
                Debug.Log($"<color=grey>  No more beans at pile, ember stays at pot</color>");
            }
        }

        Debug.Log($"<color=green>✓ Bean delivery processing complete (PotController will destroy)</color>");
    }

    public bool HasCarrier()
    {
        return hasCarrier;
    }

    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (isBeingCarried && pot != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, pot.transform.position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnPosition, 0.2f);
    }
}