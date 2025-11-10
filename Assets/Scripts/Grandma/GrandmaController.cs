using System;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
[RequireComponent(typeof(NavMeshAgent))]
public class GrandmaController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool rotateTowardMovement = true;

    [Header("Slope Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.3f;

    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool useCameraRelativeMovement = true;

    [Header("NavMesh Settings")]
    [SerializeField] private bool useNavMeshAgent = true;
    [SerializeField] private bool showDebugInfo = true;

    private NavMeshAgent agent;
    private Vector3 moveDirection;
    private bool isGrounded;
    private float currentSlopeAngle;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        ConfigureNavMeshAgent();

        Debug.Log("<color=green>✓ Improved Player Movement initialized</color>");
        Debug.Log($"  Using NavMeshAgent: {useNavMeshAgent}");
        Debug.Log($"  Camera Relative Movement: {useCameraRelativeMovement}");
    }

    private void ConfigureNavMeshAgent()
    {
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = rotationSpeed * 50f; // NavMesh uses degrees/sec
            agent.acceleration = 20f;
            agent.stoppingDistance = 0f;
            agent.autoBraking = false;
            agent.updateRotation = false; // We'll handle rotation manually for better control
            agent.updatePosition = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

            Debug.Log("<color=cyan>NavMeshAgent configured for smooth movement</color>");
        }
    }

    private void Update()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Check ground
        CheckGround();

        // Move
        if (horizontal != 0 || vertical != 0)
        {
            MovePlayer(horizontal, vertical);
        }
        else if (useNavMeshAgent && agent != null)
        {
            // Stop agent if no input
            agent.velocity = Vector3.zero;
        }

        // Debug display
        if (showDebugInfo && Input.GetKeyDown(KeyCode.I))
        {
            ShowDebugInfo();
        }
    }

    private void MovePlayer(float horizontal, float vertical)
    {
        // Calculate movement direction
        Vector3 inputDirection = Vector3.zero;

        if (useCameraRelativeMovement && cameraTransform != null)
        {
            // Camera-relative movement
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Flatten on Y axis (for isometric/top-down)
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            inputDirection = (cameraRight * horizontal + cameraForward * vertical).normalized;
        }
        else
        {
            // World-space movement
            inputDirection = new Vector3(horizontal, 0, vertical).normalized;
        }

        if (inputDirection.magnitude > 0.1f)
        {
            moveDirection = inputDirection;

            if (useNavMeshAgent && agent != null && agent.isOnNavMesh)
            {
                // Use NavMeshAgent for movement (handles slopes automatically!)
                Vector3 targetPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
                agent.SetDestination(targetPosition);

                // Manual rotation for better control
                if (rotateTowardMovement)
                {
                    RotateToward(moveDirection);
                }
            }
            else
            {
                // Fallback to manual movement with slope adjustment
                Vector3 movement = AdjustMovementForSlope(moveDirection * moveSpeed * Time.deltaTime);
                transform.position += movement;

                if (rotateTowardMovement)
                {
                    RotateToward(moveDirection);
                }
            }
        }
    }

    private Vector3 AdjustMovementForSlope(Vector3 movement)
    {
        // Raycast to check slope
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance + 0.2f, groundLayer))
        {
            // Calculate slope angle
            currentSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            if (currentSlopeAngle > 0 && currentSlopeAngle <= maxSlopeAngle)
            {
                // Project movement onto slope
                Vector3 slopeDirection = Vector3.ProjectOnPlane(movement, hit.normal).normalized;
                return slopeDirection * movement.magnitude;
            }
            else if (currentSlopeAngle > maxSlopeAngle)
            {
                // Too steep - don't move in this direction
                Debug.LogWarning($"Slope too steep: {currentSlopeAngle:F1}° (max: {maxSlopeAngle}°)");
                return Vector3.zero;
            }
        }

        return movement;
    }

    private void RotateToward(Vector3 direction)
    {
        if (direction.magnitude < 0.1f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void CheckGround()
    {
        // Raycast down to check if grounded
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            currentSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        }
    }

    private void ShowDebugInfo()
    {
        Debug.Log("<color=cyan>=== PLAYER MOVEMENT DEBUG ===</color>");
        Debug.Log($"Position: {transform.position}");
        Debug.Log($"Is Grounded: {isGrounded}");
        Debug.Log($"Current Slope Angle: {currentSlopeAngle:F1}°");
        Debug.Log($"Move Speed: {moveSpeed}");

        if (agent != null)
        {
            Debug.Log($"On NavMesh: {agent.isOnNavMesh}");
            Debug.Log($"Agent Velocity: {agent.velocity.magnitude:F2}");
            Debug.Log($"Has Path: {agent.hasPath}");
        }
    }

    // Public methods for external control
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }

    public bool IsMoving()
    {
        if (useNavMeshAgent && agent != null)
        {
            return agent.velocity.magnitude > 0.1f;
        }
        return moveDirection.magnitude > 0.1f;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetCurrentSlopeAngle()
    {
        return currentSlopeAngle;
    }

    // Gizmos for debugging
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw ground check ray
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);

        // Draw movement direction
        if (moveDirection.magnitude > 0.1f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, moveDirection * 2f);
        }

        // Draw NavMesh path
        if (useNavMeshAgent && agent != null && agent.hasPath)
        {
            Gizmos.color = Color.cyan;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw slope angle indicator
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1f);

        // Draw forward direction
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
}