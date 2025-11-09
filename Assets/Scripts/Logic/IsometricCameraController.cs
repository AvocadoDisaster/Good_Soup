using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // The player or object to follow

    [Header("Camera Angle (Choose Preset or Custom)")]
    [SerializeField] private CameraPreset preset = CameraPreset.CultOfTheLamb;
    [SerializeField] private bool useCustomAngle = false;
    [SerializeField] private Vector3 customOffset = new Vector3(0, 15, -10);

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private bool smoothFollow = true;

    [Header("Zoom Settings")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float minZoom = 8f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private KeyCode zoomInKey = KeyCode.Equals;
    [SerializeField] private KeyCode zoomOutKey = KeyCode.Minus;

    [Header("Camera Bounds (Optional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-50, -50);
    [SerializeField] private Vector2 maxBounds = new Vector2(50, 50);

    [Header("Camera Shake")]
    [SerializeField] private float shakeDecay = 2f;

    private Vector3 offset;
    private float currentZoom;
    private Vector3 shakeOffset = Vector3.zero;
    private float shakeMagnitude = 0f;

    public enum CameraPreset
    {
        CultOfTheLamb,      // 45-50 degree angle, close
        DontStarveTogether, // Higher angle, more top-down
        Custom
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera target not assigned! Please assign a target in the inspector.");
            return;
        }

        // Set camera angle based on preset
        SetCameraPreset();

        // Set initial zoom
        currentZoom = offset.magnitude;

        // Position camera immediately at start
        transform.position = target.position + offset;
        transform.LookAt(target);

        Debug.Log($"<color=cyan>Camera initialized with {preset} preset</color>");
        Debug.Log($"Camera angle: {transform.eulerAngles}");
    }

    private void SetCameraPreset()
    {
        if (useCustomAngle)
        {
            offset = customOffset;
            return;
        }

        switch (preset)
        {
            case CameraPreset.CultOfTheLamb:
                // Cult of the Lamb style: ~45 degree angle, moderate distance
                offset = new Vector3(0, 12, -10);
                break;

            case CameraPreset.DontStarveTogether:
                // Don't Starve Together: Higher, more top-down
                offset = new Vector3(0, 18, -8);
                break;

            case CameraPreset.Custom:
                offset = customOffset;
                break;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Handle zoom
        if (enableZoom)
        {
            HandleZoom();
        }

        // Calculate target position with offset
        Vector3 targetPosition = target.position + offset.normalized * currentZoom;

        // Apply camera shake
        if (shakeMagnitude > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeMagnitude -= shakeDecay * Time.deltaTime;
            if (shakeMagnitude < 0) shakeMagnitude = 0;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }

        targetPosition += shakeOffset;

        // Apply bounds if enabled
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);
        }

        // Move camera
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }

        // Always look at target
        transform.LookAt(target.position);
    }

    private void HandleZoom()
    {
        // Keyboard zoom
        if (Input.GetKey(zoomInKey))
        {
            currentZoom -= zoomSpeed * Time.deltaTime;
        }
        if (Input.GetKey(zoomOutKey))
        {
            currentZoom += zoomSpeed * Time.deltaTime;
        }

        // Mouse scroll wheel zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentZoom -= scroll * zoomSpeed * 5f;
        }

        // Clamp zoom
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    /// <summary>
    /// Shake the camera for impact effects
    /// </summary>
    public void Shake(float magnitude, float duration)
    {
        shakeMagnitude = magnitude;
        Invoke(nameof(StopShake), duration);
    }

    private void StopShake()
    {
        shakeMagnitude = 0;
    }

    /// <summary>
    /// Change camera preset at runtime
    /// </summary>
    public void SetPreset(CameraPreset newPreset)
    {
        preset = newPreset;
        SetCameraPreset();
        currentZoom = offset.magnitude;
    }

    /// <summary>
    /// Set custom offset at runtime
    /// </summary>
    public void SetCustomOffset(Vector3 newOffset)
    {
        customOffset = newOffset;
        offset = newOffset;
        currentZoom = offset.magnitude;
    }

    // Gizmos to visualize camera setup in editor
    private void OnDrawGizmos()
    {
        if (target == null) return;

        // Draw line from camera to target
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);

        // Draw bounds if enabled
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 bottomLeft = new Vector3(minBounds.x, 0, minBounds.y);
            Vector3 bottomRight = new Vector3(maxBounds.x, 0, minBounds.y);
            Vector3 topLeft = new Vector3(minBounds.x, 0, maxBounds.y);
            Vector3 topRight = new Vector3(maxBounds.x, 0, maxBounds.y);

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Show zoom range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, minZoom);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, maxZoom);
    }
}
