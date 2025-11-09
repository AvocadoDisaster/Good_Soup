using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class Bilboading : MonoBehaviour
{ 
[Header("Target")]
    [SerializeField] private Transform target;

[Header("Camera Style")]
[SerializeField] private CameraStyle style = CameraStyle.PikminCultMix;

[Header("Distance & Height")]
[SerializeField] private float distance = 15f;
[SerializeField] private float height = 12f;
[SerializeField] private float minDistance = 8f;
[SerializeField] private float maxDistance = 25f;

[Header("Angle")]
[SerializeField] private float pitchAngle = 50f; // 45-55° like Pikmin/Cult
[SerializeField] private float minPitch = 30f;
[SerializeField] private float maxPitch = 70f;

[Header("Rotation")]
[SerializeField] private bool allowRotation = true;
[SerializeField] private float rotationSpeed = 100f;
[SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
[SerializeField] private KeyCode rotateRightKey = KeyCode.E;
[SerializeField] private bool mouseRotation = true;
[SerializeField] private KeyCode mouseRotateButton = KeyCode.Mouse2; // Middle mouse

[Header("Zoom")]
[SerializeField] private bool allowZoom = true;
[SerializeField] private float zoomSpeed = 5f;
[SerializeField] private bool smoothZoom = true;

[Header("Follow Settings")]
[SerializeField] private float followSpeed = 5f;
[SerializeField] private bool smoothFollow = true;
[SerializeField] private Vector3 targetOffset = Vector3.zero;

[Header("Collision")]
[SerializeField] private bool avoidObstacles = true;
[SerializeField] private LayerMask collisionLayers;
[SerializeField] private float collisionBuffer = 0.5f;

[Header("Camera Bounds (Optional)")]
[SerializeField] private bool useBounds = false;
[SerializeField] private Vector2 minBounds = new Vector2(-50, -50);
[SerializeField] private Vector2 maxBounds = new Vector2(50, 50);

public enum CameraStyle
{
    Pikmin,          // Lower angle, more action-oriented
    CultOfTheLamb,   // Medium angle, balanced
    PikminCultMix    // Best of both!
}

private float currentYaw = 0f;
private float currentDistance;
private float targetDistance;
private Vector3 currentVelocity;
private bool isRotatingWithMouse = false;

private void Start()
{
    if (target == null)
    {
        Debug.LogError("Camera target not assigned!");
        return;
    }

    ApplyCameraStyle();
    currentDistance = distance;
    targetDistance = distance;

    // Initialize yaw to face forward
    currentYaw = target.eulerAngles.y;

    PositionCamera();

    Debug.Log($"<color=cyan>Pikmin-Style Camera initialized ({style})</color>");
    Debug.Log($"  Rotate: Q/E or Middle Mouse");
    Debug.Log($"  Zoom: Mouse Wheel");
}

private void ApplyCameraStyle()
{
    switch (style)
    {
        case CameraStyle.Pikmin:
            pitchAngle = 45f;
            distance = 12f;
            height = 8f;
            break;

        case CameraStyle.CultOfTheLamb:
            pitchAngle = 50f;
            distance = 15f;
            height = 12f;
            break;

        case CameraStyle.PikminCultMix:
            pitchAngle = 48f;
            distance = 14f;
            height = 10f;
            break;
    }

    currentDistance = distance;
    targetDistance = distance;
}

private void Update()
{
    if (target == null) return;

    HandleRotation();
    HandleZoom();
}

private void LateUpdate()
{
    if (target == null) return;

    PositionCamera();
}

private void HandleRotation()
{
    if (!allowRotation) return;

    float rotationInput = 0f;

    // Keyboard rotation
    if (Input.GetKey(rotateLeftKey))
    {
        rotationInput -= 1f;
    }
    if (Input.GetKey(rotateRightKey))
    {
        rotationInput += 1f;
    }

    // Mouse rotation
    if (mouseRotation)
    {
        if (Input.GetKeyDown(mouseRotateButton))
        {
            isRotatingWithMouse = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Input.GetKeyUp(mouseRotateButton))
        {
            isRotatingWithMouse = false;
            Cursor.lockState = CursorLockMode.None;
        }

        if (isRotatingWithMouse)
        {
            rotationInput += Input.GetAxis("Mouse X") * 2f;
        }
    }

    // Apply rotation
    if (rotationInput != 0f)
    {
        currentYaw += rotationInput * rotationSpeed * Time.deltaTime;
    }
}

private void HandleZoom()
{
    if (!allowZoom) return;

    float scrollInput = Input.GetAxis("Mouse ScrollWheel");

    if (scrollInput != 0f)
    {
        targetDistance -= scrollInput * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    // Smooth zoom
    if (smoothZoom)
    {
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 10f);
    }
    else
    {
        currentDistance = targetDistance;
    }
}

private void PositionCamera()
{
    // Calculate target position
    Vector3 targetPosition = target.position + targetOffset;

    // Calculate camera position based on angle and distance
    Quaternion rotation = Quaternion.Euler(pitchAngle, currentYaw, 0);
    Vector3 offset = rotation * new Vector3(0, 0, -currentDistance);

    Vector3 desiredPosition = targetPosition + offset;

    // Handle collision
    if (avoidObstacles)
    {
        RaycastHit hit;
        Vector3 direction = desiredPosition - targetPosition;

        if (Physics.Raycast(targetPosition, direction.normalized, out hit, direction.magnitude, collisionLayers))
        {
            desiredPosition = hit.point - direction.normalized * collisionBuffer;
        }
    }

    // Apply bounds
    if (useBounds)
    {
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
        desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);
    }

    // Move camera
    if (smoothFollow)
    {
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSpeed);
    }
    else
    {
        transform.position = desiredPosition;
    }

    // Look at target
    transform.LookAt(targetPosition);
}

// Public methods
public void SetTarget(Transform newTarget)
{
    target = newTarget;
}

public void SetStyle(CameraStyle newStyle)
{
    style = newStyle;
    ApplyCameraStyle();
}

public void RotateCamera(float degrees)
{
    currentYaw += degrees;
}

public void SetZoom(float newDistance)
{
    targetDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
}

public Vector3 GetCameraForward()
{
    Vector3 forward = transform.forward;
    forward.y = 0;
    return forward.normalized;
}

public Vector3 GetCameraRight()
{
    Vector3 right = transform.right;
    right.y = 0;
    return right.normalized;
}

public float GetCurrentYaw()
{
    return currentYaw;
}

// Debug
private void OnDrawGizmos()
{
    if (target == null || !Application.isPlaying) return;

    // Draw line from camera to target
    Gizmos.color = Color.cyan;
    Gizmos.DrawLine(transform.position, target.position);

    // Draw camera bounds
    if (useBounds)
    {
        Gizmos.color = Color.yellow;
        Vector3 bl = new Vector3(minBounds.x, target.position.y, minBounds.y);
        Vector3 br = new Vector3(maxBounds.x, target.position.y, minBounds.y);
        Vector3 tl = new Vector3(minBounds.x, target.position.y, maxBounds.y);
        Vector3 tr = new Vector3(maxBounds.x, target.position.y, maxBounds.y);

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }

    // Draw forward direction
    Gizmos.color = Color.red;
    Gizmos.DrawRay(target.position, GetCameraForward() * 3f);
}

private void OnGUI()
{
    if (!Application.isPlaying) return;

    GUILayout.BeginArea(new Rect(10, Screen.height - 100, 300, 100));
    GUILayout.Label($"Camera Angle: {pitchAngle:F1}°");
    GUILayout.Label($"Distance: {currentDistance:F1}m");
    GUILayout.Label($"Rotation: {currentYaw:F1}°");
    GUILayout.EndArea();
}
}
