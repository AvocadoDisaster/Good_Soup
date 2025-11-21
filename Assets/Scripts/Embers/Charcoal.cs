using UnityEngine;
using UnityEngine.AI;

public class Charcoal : MonoBehaviour
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
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            Debug.LogWarning($"<color=yellow>Charcoal {gameObject.name} has no Rigidbody!</color>");
        }

        spawnPosition = transform.position;
        Debug.Log($"<color=orange>Charcoal Awake: {gameObject.name}</color>");
    }

    private void Start()
    {
        if (pot == null)
        {
            pot = GameObject.FindGameObjectWithTag("Pot");
        }

        Debug.Log($"<color=orange>Charcoal spawned at {spawnPosition}</color>");
    }

    private void Update()
    {
        // CRITICAL: Check state FIRST every frame if we have a carrier
        if (carrierEmber != null)
        {
            EmberState emberState = EmberStateManager.Instance.GetEmberState(carrierEmber.gameObject);

            // Drop immediately if ember is rallied or thrown
            if (emberState == EmberState.IN_RALLY_PARTY ||
                emberState == EmberState.BEING_THROWN ||
                emberState != EmberState.ON_CHARCOAL)
            {
                Debug.Log($"<color=red>CHARCOAL: Ember state changed to {emberState} - DROPPING IMMEDIATELY!</color>");
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

        if (EmberStateManager.Instance.TryClaimEmber(ember.gameObject, EmberState.ON_CHARCOAL))
        {
            carrierEmber = ember;
            isBeingCarried = true;
            hasCarrier = true;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.enabled = false;
            }

            Debug.Log($"<color=orange>{gameObject.name} picked up by {ember.gameObject.name}</color>");
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

                agent.speed = carrySpeed;
                agent.acceleration = 8f;
                agent.angularSpeed = 120f;
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.updatePosition = true;
                agent.SetDestination(pot.transform.position);

                Debug.Log($"<color=orange>Charcoal being carried to pot at speed {carrySpeed}</color>");
            }
        }
    }

    private void UpdateCarrying()
    {
        if (carrierEmber == null)
        {
            Drop();
            return;
        }

        EmberState emberState = EmberStateManager.Instance.GetEmberState(carrierEmber.gameObject);
        if (emberState != EmberState.ON_CHARCOAL)
        {
            Debug.Log($"<color=yellow>Ember changed state to {emberState}, DROPPING charcoal!</color>");
            Drop();
            return;
        }

        NavMeshAgent agent = carrierEmber.GetAgent();
        if (agent != null && pot != null)
        {
            if (agent.isStopped)
            {
                Debug.LogWarning($"<color=yellow>Charcoal agent was stopped! Re-enabling...</color>");
                agent.isStopped = false;
            }

            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                agent.SetDestination(pot.transform.position);
            }

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

        Vector3 targetPosition = carrierEmber.transform.position + Vector3.up * hoverHeight;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

        if (pot != null)
        {
            float distanceToPot = Vector3.Distance(transform.position, pot.transform.position);

            if (distanceToPot < 5f && Time.frameCount % 30 == 0)
            {
                Debug.Log($"<color=orange>Charcoal approaching pot! Distance: {distanceToPot:F2}</color>");
            }

            if (distanceToPot < 2f)
            {
                Debug.Log($"<color=green>Charcoal reached pot! Distance: {distanceToPot:F2} - Triggering delivery!</color>");
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
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        Debug.Log($"<color=yellow>✓ Charcoal dropped at position {transform.position}</color>");
    }

    private void DeliverToPot()
    {
        Debug.Log($"<color=green>========== CHARCOAL DELIVERY TO POT ==========</color>");
        Debug.Log($"  Charcoal position: {transform.position}");
        Debug.Log($"  Pot position: {(pot != null ? pot.transform.position.ToString() : "NULL")}");

        EmberBehavior deliveryEmber = carrierEmber;
        Vector3 returnPosition = spawnPosition;

        // Try to find and call PotController
        PotController potController = null;
        if (pot != null)
        {
            potController = pot.GetComponent<PotController>();
            if (potController != null)
            {
                Debug.Log($"<color=green>  PotController found! Calling ReceiveCharcoal...</color>");
                potController.GetType().GetMethod("ReceiveCharcoal",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.Invoke(potController, new object[] { this });
            }
            else
            {
                Debug.LogError($"<color=red>  PotController NOT FOUND on {pot.name}!</color>");
            }
        }

        // Release carrier ember and check for more charcoal
        if (deliveryEmber != null)
        {
            EmberStateManager.Instance.ReleaseEmber(deliveryEmber.gameObject);

            // Check if there are more charcoal pieces at the spawn location
            Collider[] nearbyColl = Physics.OverlapSphere(returnPosition, 5f);
            bool hasMoreCharcoal = false;
            foreach (Collider col in nearbyColl)
            {
                Charcoal charcoal = col.GetComponent<Charcoal>();
                if (charcoal != null && charcoal != this && !charcoal.HasCarrier())
                {
                    hasMoreCharcoal = true;
                    Debug.Log($"<color=cyan>  Found more charcoal at pile: {charcoal.gameObject.name}</color>");
                    break;
                }
            }

            // Only send ember back if there's more charcoal
            if (hasMoreCharcoal)
            {
                NavMeshAgent agent = deliveryEmber.GetAgent();
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.speed = 3.5f;
                    agent.isStopped = false;
                    bool pathSet = agent.SetDestination(returnPosition);
                    Debug.Log($"<color=magenta>  Sending ember back to charcoal pile at {returnPosition}</color>");
                    Debug.Log($"  Path set successfully: {pathSet}</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>  Cannot send ember back - agent issues</color>");
                }
            }
            else
            {
                Debug.Log($"<color=grey>  No more charcoal at pile, ember stays at pot</color>");
            }
        }

        Debug.Log($"<color=green>✓ Charcoal delivery complete!</color>");
    }

    public bool HasCarrier()
    {
        return hasCarrier;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (isBeingCarried && pot != null)
        {
            
            Gizmos.DrawLine(transform.position, pot.transform.position);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnPosition, 0.2f);
    }
}