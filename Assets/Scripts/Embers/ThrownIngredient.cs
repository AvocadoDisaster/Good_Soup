using UnityEngine;
using System.Collections;

public class ThrowController : MonoBehaviour
{
    public bool IsFlying { get; private set; } = false;

    private bool isActive = false;
    private Coroutine throwCoroutine = null;

    public void StartThrow(Vector3 startPos, Vector3 direction, float v0, float angle, float flightTime, float lifetime)
    {
        if (isActive)
        {
            Debug.LogWarning($"ThrowController on {gameObject.name} is already active!");
            return;
        }

        isActive = true;
        IsFlying = true;  // IMPORTANT
        throwCoroutine = StartCoroutine(ThrowCoroutine(startPos, direction, v0, angle, flightTime, lifetime));
    }

    public void StopThrow()
    {
        isActive = false;
        IsFlying = false; // IMPORTANT

        if (throwCoroutine != null)
        {
            StopCoroutine(throwCoroutine);
            throwCoroutine = null;
        }
    }

    private IEnumerator ThrowCoroutine(Vector3 startPos, Vector3 direction, float v0, float angle, float flightTime, float lifetime)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>();

        transform.position = startPos;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (col != null)
            col.enabled = false;

        float t = 0;

        // FLIGHT (IsFlying = true)
        while (t < flightTime && isActive)
        {
            float x = v0 * t * Mathf.Cos(angle);
            float y = v0 * t * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * (t * t);

            transform.position = startPos + direction * x + Vector3.up * y;

            t += Time.deltaTime;
            yield return null;
        }

        if (!isActive)
            yield break;

        // LANDING → IsFlying = false
        IsFlying = false;

        if (col != null)
            col.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.down * 2f;
        }

        // Cleanup timer
        float timer = 0;
        while (timer < lifetime && isActive && gameObject != null)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (isActive && transform.parent == null)
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        StopThrow();
    }
}
