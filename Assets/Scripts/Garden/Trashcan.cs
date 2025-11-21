using UnityEngine;

public class TrashCan : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem destroyEffect; // Optional particle effect
    [SerializeField] private AudioClip trashSound; // Optional sound effect
    private AudioSource audioSource;

    private void Start()
    {
        // Get or add audio source for sound effects
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && trashSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Make sure this has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Accept ANY thrown item (ingredients, embers, trash)
        if (other.CompareTag("Ingredient") || other.CompareTag("Ember") || other.CompareTag("Trash"))
        {
            Debug.Log($"<color=orange>🗑️ TrashCan: {other.gameObject.name} disposed safely!</color>");

            // Visual feedback
            if (destroyEffect != null)
            {
                Instantiate(destroyEffect, other.transform.position, Quaternion.identity);
            }

            // Sound feedback
            if (audioSource != null && trashSound != null)
            {
                audioSource.PlayOneShot(trashSound);
            }

            // Simply destroy the item - NO penalty, NO scoring
            Destroy(other.gameObject);
        }
    }

    // Visual helper in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange transparent
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}