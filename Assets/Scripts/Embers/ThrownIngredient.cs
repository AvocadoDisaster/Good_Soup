using UnityEngine;
using System.Collections;

/// <summary>
/// ThrowController - Handles individual ingredient throw physics.
/// Each ingredient gets its OWN instance to prevent interference.
/// </summary>
public class ThrowController : MonoBehaviour
{
    private bool isThisIngredientFlying = false;
    private Coroutine myThrowCoroutine = null;
    private GameObject myGameObject;
    private int instanceID;

    // Public property to check if this ingredient is flying
    public bool IsFlying => isThisIngredientFlying;

    private void Awake()
    {
        myGameObject = gameObject;
        instanceID = GetInstanceID();
        Debug.Log($"<color=magenta>ThrowController created for {myGameObject.name} (ID: {instanceID})</color>");

        // CRITICAL: Destroy any LineRenderer immediately
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            Destroy(lr);
            Debug.Log($"  - Destroyed LineRenderer in ThrowController.Awake()");
        }
    }

    public void StartThrow(Vector3 startPos, Vector3 direction, float v0, float angle, float flightTime, float lifetime)
    {
        if (isThisIngredientFlying)
        {
            Debug.LogError($"ThrowController on {myGameObject.name} (ID: {instanceID}) is ALREADY flying! This shouldn't happen!");
            return;
        }

        Debug.Log($"<color=cyan>▶ StartThrow for {myGameObject.name} (ID: {instanceID})</color>");

        // SAFETY: Destroy LineRenderer one more time
        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr != null)
        {
            Destroy(lr);
            Debug.Log($"  - Destroyed LineRenderer in StartThrow()");
        }

        isThisIngredientFlying = true;

        // Stop any existing coroutine just to be safe
        if (myThrowCoroutine != null)
        {
            StopCoroutine(myThrowCoroutine);
        }

        myThrowCoroutine = StartCoroutine(ThrowCoroutine(startPos, direction, v0, angle, flightTime, lifetime));
    }

    public void StopThrow()
    {
        Debug.Log($"<color=yellow>■ StopThrow for {myGameObject.name} (ID: {instanceID})</color>");
        isThisIngredientFlying = false;

        if (myThrowCoroutine != null)
        {
            StopCoroutine(myThrowCoroutine);
            myThrowCoroutine = null;
        }
    }

    private IEnumerator ThrowCoroutine(Vector3 startPos, Vector3 direction, float v0, float angle, float flightTime, float lifetime)
    {
        // Store references at the START to ensure we're always working with THIS ingredient
        Transform myTransform = transform;
        Rigidbody myRb = GetComponent<Rigidbody>();
        Collider myCol = GetComponent<Collider>();

        Debug.Log($"<color=cyan>  ✈ ThrowCoroutine STARTED for {myGameObject.name} (ID: {instanceID})</color>");
        Debug.Log($"    Start pos: {startPos}");
        Debug.Log($"    Direction: {direction}");
        Debug.Log($"    v0: {v0}, angle: {angle}, time: {flightTime}");
        Debug.Log($"    Current position: {myTransform.position}");
        Debug.Log($"    Current parent: {(myTransform.parent != null ? myTransform.parent.name : "NULL")}");

        // Set initial position IMMEDIATELY
        myTransform.position = startPos;
        Debug.Log($"    Set position to startPos: {myTransform.position}");

        // Prepare for flight
        if (myRb != null)
        {
            myRb.isKinematic = true;
            myRb.useGravity = false;
            myRb.linearVelocity = Vector3.zero;
            myRb.angularVelocity = Vector3.zero;
            Debug.Log($"    Rigidbody configured for flight");
        }
        else
        {
            Debug.LogError($"    ERROR: No Rigidbody found on {myGameObject.name}!");
        }

        if (myCol != null)
        {
            myCol.enabled = false;
            Debug.Log($"    Collider disabled");
        }

        float t = 0;
        int frameCount = 0;

        Debug.Log($"<color=cyan>    Starting flight loop...</color>");

        // FLIGHT PHASE
        while (t < flightTime && isThisIngredientFlying && myGameObject != null)
        {
            float x = v0 * t * Mathf.Cos(angle);
            float y = v0 * t * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(t, 2);

            Vector3 newPos = startPos + direction * x + Vector3.up * y;
            myTransform.position = newPos;

            // Debug every 10 frames
            if (frameCount % 10 == 0)
            {
                Debug.Log($"    Frame {frameCount}: t={t:F2}, pos={newPos}, parent={(myTransform.parent != null ? myTransform.parent.name : "NULL")}");
            }

            t += Time.deltaTime;
            frameCount++;
            yield return null;
        }

        Debug.Log($"<color=yellow>    Flight loop ended. Frames: {frameCount}, Final t: {t:F2}</color>");

        // Check if we were interrupted
        if (!isThisIngredientFlying || myGameObject == null)
        {
            Debug.Log($"<color=yellow>  ⊗ Flight cancelled for {myGameObject.name} (ID: {instanceID})</color>");
            yield break;
        }

        // LANDING PHASE
        Debug.Log($"<color=lime>  ↓ {myGameObject.name} (ID: {instanceID}) LANDED at {myTransform.position}</color>");

        if (myCol != null)
        {
            myCol.enabled = true;
            Debug.Log($"    Collider enabled");
        }

        if (myRb != null)
        {
            myRb.isKinematic = false;
            myRb.useGravity = true;
            myRb.linearVelocity = Vector3.down * 2f;
            Debug.Log($"    Physics enabled, adding downward velocity");
        }

        isThisIngredientFlying = false;

        // CLEANUP PHASE
        float cleanupTimer = 0;
        while (cleanupTimer < lifetime && myGameObject != null && myTransform.parent == null)
        {
            cleanupTimer += Time.deltaTime;
            yield return null;
        }

        // Destroy if still not picked up
        if (myGameObject != null && myTransform.parent == null)
        {
            Debug.Log($"<color=orange>  ✖ Destroying missed ingredient: {myGameObject.name} (ID: {instanceID})</color>");
            Destroy(myGameObject);
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"<color=red>ThrowController destroyed for {myGameObject.name} (ID: {instanceID})</color>");
        StopThrow();
    }
}