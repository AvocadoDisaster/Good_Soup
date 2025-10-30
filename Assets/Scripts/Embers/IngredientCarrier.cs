using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class IngredientCarrier : MonoBehaviour
{
    [Header("Ingredient Settings")]
    public string ingredientName;
    public Sprite ingredientIcon;

    [Header("Carrying Settings")]
    [SerializeField] private int minEmbersToMove = 3;
    [SerializeField] private int maxEmbers = 9;
    [SerializeField] private float radiusAroundIngredient = 1.5f;
    [SerializeField] private float hoverHeight = 1f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("References")]
    [SerializeField] private GameObject pot;

   [SerializeField] public List<EmberBehavior> attachedEmbers = new List<EmberBehavior>();
    private Vector3 targetPosition;
    private bool isBeingCarried = false;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void Start()
    {
        if (pot == null)
        {
            pot = GameObject.FindGameObjectWithTag("Pot");

            if (pot == null)
            {
                Debug.LogError($"{ingredientName}: Cannot find Pot! Make sure a GameObject has the 'Pot' tag.");
            }
            else
            {
                // Check if pot is at a reasonable height
                if (pot.transform.position.y > 10f)
                {
                    Debug.LogError($"{ingredientName}: Pot is at Y={pot.transform.position.y}! It should be near ground level (Y < 5). This will cause NavMesh pathing to fail!");
                }
                else
                {
                    Debug.Log($"{ingredientName}: Found pot at {pot.transform.position}");
                }
            }
        }
    }

    private void Update()
    {
        //  Only update if actually being carried AND have enough embers
        if (!isBeingCarried)
        {
            return;
        }

        if (attachedEmbers.Count < minEmbersToMove)
        {
            StopCarrying();
            return;
        }

        // Remove null embers from list
        attachedEmbers.RemoveAll(ember => ember == null);

        if (attachedEmbers.Count < minEmbersToMove)
        {
            StopCarrying();
            return;
        }

        UpdateCarrying();

        // Debug: Press G to see status
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log($"=== {ingredientName} STATUS ===");
            Debug.Log($"Being Carried: {isBeingCarried}");
            Debug.Log($"Ember Count: {attachedEmbers.Count}");
            Debug.Log($"Speed: {GetCarrySpeed()}");
            Debug.Log($"Position: {transform.position}");
            if (pot != null)
            {
                Debug.Log($"Pot Position: {pot.transform.position}");
                Debug.Log($"Distance to Pot: {Vector3.Distance(transform.position, pot.transform.position):F2}");
            }
            else
            {
                Debug.LogError("POT IS NULL!");
            }

            for (int i = 0; i < attachedEmbers.Count; i++)
            {
                if (attachedEmbers[i] != null)
                {
                    NavMeshAgent agent = attachedEmbers[i].GetAgent();
                    if (agent != null)
                    {
                        Debug.Log($"  Ember {i}: Enabled={agent.enabled}, OnNavMesh={agent.isOnNavMesh}, Speed={agent.speed}, HasPath={agent.hasPath}, Velocity={agent.velocity.magnitude:F2}, Destination={agent.destination}");
                    }
                }
            }
        }
    }

    private void UpdateCarrying()
    {
        // Calculate average position of all embers
        Vector3 centerPosition = Vector3.zero;
        int activeEmbers = 0;

        foreach (EmberBehavior ember in attachedEmbers)
        {
            if (ember != null)
            {
                centerPosition += ember.transform.position;
                activeEmbers++;
            }
        }

        if (activeEmbers == 0)
        {
            StopCarrying();
            return;
        }

        centerPosition /= activeEmbers;

        // Position ingredient above embers
        targetPosition = centerPosition + Vector3.up * hoverHeight;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

        // Rotate to match ground slope
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, hoverHeight + 2f))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Position embers in circle and move them toward pot
        MoveEmbersTowardPot();
    }

    private void MoveEmbersTowardPot()
    {
        if (pot == null)
        {
            Debug.LogError($"{ingredientName}: POT IS NULL! Cannot move embers.");
            return;
        }

        // Calculate speed based on ember count
        float speed = GetCarrySpeed();

        if (speed <= 0)
        {
            Debug.LogWarning($"{ingredientName}: Speed is 0! Not enough embers?");
            return;
        }

        for (int i = 0; i < attachedEmbers.Count; i++)
        {
            if (attachedEmbers[i] == null) continue;

            NavMeshAgent agent = attachedEmbers[i].GetAgent();
            if (agent == null)
            {
                Debug.LogError($"Ember {i} has no NavMeshAgent!");
                continue;
            }

            // Re-enable agent if disabled
            if (!agent.enabled)
            {
                agent.enabled = true;
                Debug.Log($"Re-enabled NavMeshAgent for ember {i}");
            }

            // Check if on NavMesh
            if (!agent.isOnNavMesh)
            {
                Debug.LogError($"Ember {i} NOT on NavMesh! Position: {attachedEmbers[i].transform.position}");

                // Try to warp to NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(attachedEmbers[i].transform.position, out hit, 2f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    Debug.Log($"Warped ember {i} to NavMesh at {hit.position}");
                }
                else
                {
                    Debug.LogError($"Cannot find NavMesh near ember {i}!");
                    continue;
                }
            }

            // Configure agent for carrying
            agent.speed = speed;
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;

            // Set destination to pot
            bool pathSet = agent.SetDestination(pot.transform.position);

            if (!pathSet)
            {
                Debug.LogError($"Failed to set destination for ember {i}! Pot at: {pot.transform.position}");
            }
            else if (!agent.hasPath)
            {
                Debug.LogWarning($"Ember {i} has no path to pot! PathStatus: {agent.pathStatus}");
            }
        }
    }

    public bool CanAddEmber()
    {
        return attachedEmbers.Count < maxEmbers;
    }

    public void AddEmber(EmberBehavior ember)
    {
        if (!attachedEmbers.Contains(ember))
        {
            attachedEmbers.Add(ember);
            Debug.Log($"Ember added to {ingredientName}. Total: {attachedEmbers.Count}/{minEmbersToMove}");

            if (attachedEmbers.Count >= minEmbersToMove && !isBeingCarried)
            {
                StartCarrying();
            }
        }
    }

    public void RemoveEmber(EmberBehavior ember)
    {
        if (attachedEmbers.Remove(ember))
        {
            Debug.Log($"<color=yellow>Ember removed from {ingredientName}. Remaining: {attachedEmbers.Count}/{minEmbersToMove}</color>");

            // Immediately check if we need to stop carrying
            if (attachedEmbers.Count < minEmbersToMove)
            {
                Debug.Log($"<color=red>Not enough embers! Dropping {ingredientName}</color>");
                StopCarrying();
            }
            else
            {
                // Still have enough embers, continue carrying
                Debug.Log($"<color=green>{ingredientName} still being carried by {attachedEmbers.Count} embers</color>");
            }
        }
        else
        {
            Debug.LogWarning($"Tried to remove ember that wasn't in {ingredientName}'s list!");
        }
    }

    private void StartCarrying()
    {
        isBeingCarried = true;

        // Disable physics
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable non-trigger colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            if (!col.isTrigger)
            {
                col.enabled = false;
            }
        }

        Debug.Log($"<color=green>{ingredientName} is now being carried by {attachedEmbers.Count} embers!</color>");
    }

    private void StopCarrying()
    {
        isBeingCarried = false;

        // Re-enable physics
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Re-enable colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        Debug.Log($"<color=yellow>{ingredientName} dropped (not enough embers)</color>");
    }

    public void ReleaseAllEmbers()
    {
        Debug.Log($"<color=magenta>Releasing all {attachedEmbers.Count} embers from {ingredientName}</color>");

        // Scatter embers
        foreach (EmberBehavior ember in attachedEmbers)
        {
            if (ember != null)
            {
                // Random offset
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float randomRadius = Random.Range(0.5f, 1.5f);
                Vector3 offset = new Vector3(
                    Mathf.Cos(randomAngle) * randomRadius,
                    0,
                    Mathf.Sin(randomAngle) * randomRadius
                );
                ember.transform.position = transform.position + offset;

                // Re-enable agent
                NavMeshAgent agent = ember.GetAgent();
                if (agent != null)
                {
                    agent.enabled = true;
                    agent.isStopped = false;
                }

                // Release state
                ember.DetachFromIngredient();
            }
        }

        attachedEmbers.Clear();
        isBeingCarried = false;

        Debug.Log($"<color=green>✓ All embers released from {ingredientName}</color>");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if reached pot
        if (other.CompareTag("Pot") && isBeingCarried)
        {
            PotController potController = other.GetComponent<PotController>();
            if (potController != null)
            {
                potController.ReceiveIngredient(this);
            }
        }
    }

    public int GetEmberCount()
    {
        return attachedEmbers.Count;
    }

    public float GetCarrySpeed()
    {
        // Speed based on ember count
        if (attachedEmbers.Count >= 9) return 9f;
        if (attachedEmbers.Count >= 6) return 6f;
        if (attachedEmbers.Count >= 3) return 3f;
        return 0f;
    }

    // Visual debug
    private void OnDrawGizmos()
    {
        if (isBeingCarried && pot != null)
        {
            // Draw line from ingredient to pot
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, pot.transform.position);

            // Draw circle showing hover area
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radiusAroundIngredient);
        }
    }
}