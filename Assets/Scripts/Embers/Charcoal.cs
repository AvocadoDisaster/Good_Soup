using UnityEngine;
using UnityEngine.AI;

public class Charcoal : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float hoverHeight = 0.5f;
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float carrySpeed = 6f; // Speed when being carried
    [SerializeField] private GameObject pot;

    private EmberBehavior carrierEmber;
    private bool isBeingCarried = false;
    private Rigidbody rb;
    private Vector3 spawnPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        spawnPosition = transform.position;
    }

    private void Start()
    {
        if (pot == null)
        {
            pot = GameObject.FindGameObjectWithTag("Pot");
        }
    }

    private void Update()
    {
        if (!isBeingCarried)
        {
            DetectNearbyEmber();
        }
        else if (carrierEmber != null)
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
        // Claim the ember with new ON_CHARCOAL state
        if (EmberStateManager.Instance.TryClaimEmber(ember.gameObject, EmberState.ON_CHARCOAL))
        {
            carrierEmber = ember;
            isBeingCarried = true;

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

                agent.speed = carrySpeed;
                agent.isStopped = false;
                agent.SetDestination(pot.transform.position);
                agent.destination = pot.transform.position;
                Debug.Log($"<color=orange>Charcoal being carried to pot at speed {carrySpeed}</color>");
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

        // Check ember state - if not ON_CHARCOAL, drop
        EmberState emberState = EmberStateManager.Instance.GetEmberState(carrierEmber.gameObject);
        if (emberState != EmberState.ON_CHARCOAL)
        {
            Debug.Log($"<color=yellow>Ember changed state to {emberState}, dropping charcoal</color>");
            Drop();
            return;
        }

        // Position charcoal above ember
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
            rb.isKinematic = false;
            rb.useGravity = true;
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
        Debug.Log($"<color=green> Charcoal delivered to pot!</color>");

        // Spawn new ember at pot
        PotController potController = pot.GetComponent<PotController>();
        if (potController != null)
        {
            potController.SpawnNewEmber(pot.transform.position);
        }

        // Release carrier ember
        if (carrierEmber != null)
        {
            EmberStateManager.Instance.ReleaseEmber(carrierEmber.gameObject);
        }

        // Destroy charcoal
        Destroy(gameObject);
    }
    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
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
    }
}