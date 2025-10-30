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
        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
        }
    }

    
    
}

