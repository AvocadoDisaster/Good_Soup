using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class RoughEmberPathing : MonoBehaviour
{


    private NavMeshAgent navMeshAgent;
    private Transform potTransform; // Set in the Inspector
    private Transform followTarget; // The Player or Rally Caller

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        // Find the pot in the scene. You could also assign this via the Inspector.
        potTransform = GameObject.FindGameObjectWithTag("Pot").transform;
    }

    private void Update()
    {
        // Continuous action: If following a target, update the destination.
        if (followTarget != null)
        {
            navMeshAgent.SetDestination(followTarget.position);
        }

        // If carrying an ingredient, move the group to the pot.
        if (transform.parent != null && transform.parent.CompareTag("Ingredient"))
        {
            // Check if there are at least 3 agents as children of the ingredient
            if (transform.parent.childCount >= 3)
            {
                // Move the parent (ingredient) and all its children.
                // We stop the NavMeshAgent to allow the parent's movement to take over.
                navMeshAgent.isStopped = true;
                transform.parent.position = Vector3.MoveTowards(
                    transform.parent.position,
                    potTransform.position,
                    Time.deltaTime * navMeshAgent.speed);
                

                // Check if the ingredient has reached the pot.
                if (Vector3.Distance(transform.parent.position, potTransform.position) < 1.0f)
                {
                    Destroy(transform.parent.gameObject);
                }
            }
        }
    }

    // Called when the agent is "thrown".
    public void OnThrown()
    {
        navMeshAgent.ResetPath(); // Clears the current destination.
        followTarget = null;
        if (transform.parent != null)
        {
            transform.parent = null; // Detach from any ingredient.
        }
    }

    // Called by the child trigger when colliding with the player or rally caller.
    public void OnSlappedOrRallied(Transform target)
    {
        followTarget = target;
        navMeshAgent.isStopped = false;
        if (transform.parent != null)
        {
            transform.parent = null; // Detach from any ingredient.
        }
    }

    // Called by the child trigger when detecting an ingredient.
    public void OnIngredientDetected(GameObject ingredient)
    {
        if (transform.parent == null)
        {
            // Stop agent's movement and become a child of the ingredient.
            navMeshAgent.ResetPath();
            navMeshAgent.isStopped = true;
            transform.parent = ingredient.transform;
            followTarget = null;
        }
    }
}

