using UnityEngine;
using UnityEngine.AI;

public class EmberBehavior : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private LayerMask ingredientLayer;

    private NavMeshAgent agent;
    private IngredientCarrier currentIngredient;
    private EmberState currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            agent.updateRotation = true;
            agent.updatePosition = true;
        }
    }

    private void Start()
    {
        // Register with state manager
        if (EmberStateManager.Instance != null)
        {
            EmberStateManager.Instance.RegisterEmber(gameObject);
        }
    }

    private void Update()
    {
        if (EmberStateManager.Instance == null) return;

        currentState = EmberStateManager.Instance.GetEmberState(gameObject);

        // Only FREE embers can detect and attach to things
        // NOT rallied embers (IN_RALLY_PARTY) or embers already carrying something
        if (currentState == EmberState.FREE && agent != null && agent.enabled)
        {
            DetectNearbyObjects();
        }

        // Safety check: If ON_INGREDIENT but no ingredient reference
        // Only check ON_INGREDIENT state - NOT charcoal or beans (they manage themselves)
        if (currentState == EmberState.ON_INGREDIENT && currentIngredient == null && transform.parent == null)
        {
            Debug.LogWarning($"{gameObject.name} is ON_INGREDIENT with no reference and no parent. Releasing to FREE.");
            EmberStateManager.Instance.ReleaseEmber(gameObject);
        }

        // Note: ON_CHARCOAL and ON_GARBANZO_BEANS states are managed by their respective scripts
        // They don't need safety checks here
    }

    private void DetectNearbyObjects()
    {
        // Check for Ingredients (highest priority)
        Collider[] ingredients = Physics.OverlapSphere(transform.position, detectionRadius, ingredientLayer);
        if (ingredients.Length > 0)
        {
            IngredientCarrier ingredient = ingredients[0].GetComponent<IngredientCarrier>();
            if (ingredient != null && ingredient.CanAddEmber())
            {
                AttachToIngredient(ingredient);
                return;
            }
        }

        // Charcoal and GarbanzoBean detection removed - they handle detection themselves
        // Charcoal.cs and GarbanzoBean.cs will find FREE embers and claim them
        // using ON_CHARCOAL and ON_GARBANZO_BEANS states respectively
    }

    private void AttachToIngredient(IngredientCarrier ingredient)
    {
        if (EmberStateManager.Instance.TryClaimEmber(gameObject, EmberState.ON_INGREDIENT))
        {
            currentIngredient = ingredient;
            ingredient.AddEmber(this);

            // Make sure agent stays enabled
            if (agent != null)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }

            Debug.Log($"<color=green>{gameObject.name} attached to {ingredient.gameObject.name}</color>");
        }
    }

    public void DetachFromIngredient()
    {
        if (currentIngredient != null)
        {
            currentIngredient.RemoveEmber(this);
            currentIngredient = null;
        }

        // Release to FREE state
        if (EmberStateManager.Instance != null)
        {
            EmberStateManager.Instance.ReleaseEmber(gameObject);
        }

        Debug.Log($"<color=cyan>{gameObject.name} detached from ingredient, now FREE</color>");
    }

    public void SetDestination(Vector3 destination)
    {
        if (agent != null && agent.enabled)
        {
            agent.SetDestination(destination);
        }
    }

    public void MoveToPosition(Vector3 position)
    {
        if (agent != null && agent.enabled)
        {
            agent.Move(position - transform.position);
        }
    }

    public NavMeshAgent GetAgent()
    {
        return agent;
    }

    private void OnDrawGizmosSelected()
    {
        // Show detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}