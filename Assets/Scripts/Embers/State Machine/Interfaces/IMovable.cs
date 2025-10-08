using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;

public interface IMovable 
{
   NavMeshAgent agent { get;  set; }
    bool IsFacingRight { get; set; }
    bool IsFacingbakcRight { get; set; }
    void MoveEmber (Vector3 Movement);

    void CheckForLeftorRichtFacing( NavMeshAgent agent);
    void CheckForBackLeftorRichtFacing(NavMeshAgent agent);
}
