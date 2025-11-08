using UnityEngine;

public class SimplePotTrigger : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem splashEffect;
    [SerializeField] private AudioSource scoreSound;

    [Header("Score Display")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Transform scorePopupSpawnPoint;

    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("<color=red>SimplifiedGameManager not found!</color>");
        }

        // Make sure this has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        Debug.Log("<color=green> Simple Pot Trigger initialized</color>");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=yellow>Pot trigger hit by: {other.gameObject.name}</color>");

        // Check for throwable ingredient
        ThrowableIngredient ingredient = other.GetComponent<ThrowableIngredient>();
        if (ingredient != null && ingredient.HasBeenThrown())
        {
            Debug.Log($"<color=green> INGREDIENT SCORED: {other.gameObject.name}</color>");

            // Play effects
            if (splashEffect != null)
            {
                splashEffect.Play();
            }

            if (scoreSound != null)
            {
                scoreSound.Play();
            }

            // Spawn score popup
            if (scorePopupPrefab != null && scorePopupSpawnPoint != null)
            {
                Instantiate(scorePopupPrefab, scorePopupSpawnPoint.position, Quaternion.identity);
            }

            // Notify game manager (ingredient will destroy itself)
            if (gameManager != null)
            {
                gameManager.IngredientScored();
            }
        }

        // Also check for embers (simplified - they just score too)
        if (other.CompareTag("Ember"))
        {
            Debug.Log($"<color=orange> EMBER SCORED!</color>");

            if (splashEffect != null)
            {
                splashEffect.Play();
            }

            if (scoreSound != null)
            {
                scoreSound.Play();
            }

            if (gameManager != null)
            {
                gameManager.IngredientScored();
            }

            Destroy(other.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw pot trigger area
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}
