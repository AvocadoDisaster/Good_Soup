using UnityEngine;

public class PotCollision : MonoBehaviour
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
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("<color=red>GameManager not found!</color>");
        }

        // Ensure trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        Debug.Log("<color=green>✓ Pot Trigger initialized</color>");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=yellow>Pot trigger hit by: {other.gameObject.name} (Tag: {other.tag})</color>");

        // Check if it's an EMBER (worth 2 points, doesn't count as ingredient)
        if (other.CompareTag("Ember"))
        {
            Debug.Log($"<color=orange> EMBER SCORED! +2 POINTS!</color>");

            PlayEffects();

            if (gameManager != null)
            {
                gameManager.EmberScored(); // Separate ember scoring
            }

            Destroy(other.gameObject);
            return; // Exit early so ember isn't counted as ingredient
        }

        // Check for throwable INGREDIENT (worth 1 point)
        ThrowableIngredient ingredient = other.GetComponent<ThrowableIngredient>();
        if (ingredient != null && ingredient.HasBeenThrown())
        {
            Debug.Log($"<color=green> INGREDIENT SCORED: {other.gameObject.name}</color>");

            PlayEffects();

            if (gameManager != null)
            {
                gameManager.IngredientScored(other.gameObject.name);
            }

            Destroy(other.gameObject);
            return;
        }

        // Fallback: If it has "Ingredient" tag but no ThrowableIngredient component
        if (other.CompareTag("Ingredient"))
        {
            Debug.Log($"<color=green>🎯 INGREDIENT SCORED (tag-based): {other.gameObject.name}</color>");

            PlayEffects();

            if (gameManager != null)
            {
                gameManager.IngredientScored(other.gameObject.name);
            }

            Destroy(other.gameObject);
        }
    }

    private void PlayEffects()
    {
        if (splashEffect != null)
        {
            splashEffect.Play();
        }

        if (scoreSound != null)
        {
            scoreSound.Play();
        }

        if (scorePopupPrefab != null && scorePopupSpawnPoint != null)
        {
            Instantiate(scorePopupPrefab, scorePopupSpawnPoint.position, Quaternion.identity);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}