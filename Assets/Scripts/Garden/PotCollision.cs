using UnityEngine;

public class PotCollision : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem splashEffect;
    [SerializeField] private AudioSource scoreSound;
    [SerializeField] private AudioSource trashSound;

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

        Debug.Log("<color=green> Pot Trigger initialized</color>");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=yellow>Pot trigger hit by: {other.gameObject.name} (Tag: {other.tag})</color>");

        // NEW: Check if it's TRASH (contains "trash" in name - case insensitive)
        if (other.gameObject.name.ToLower().Contains("trash"))
        {
            Debug.Log($"<color=red> TRASH ITEM DETECTED: {other.gameObject.name}</color>");
            PlayTrashEffects();

            if (gameManager != null)
            {
                gameManager.TrashCollected();
            }

            Destroy(other.gameObject);
            return; 
        }

        
        if (other.CompareTag("Ember"))
        {
            Debug.Log($"<color=orange> EMBER SCORED! +2 POINTS!</color>");
            PlayEffects();

            if (gameManager != null)
            {
                gameManager.EmberScored();
            }

            Destroy(other.gameObject);
            return; 
        }

       
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

        
        if (other.CompareTag("Ingredient"))
        {
            Debug.Log($"<color=green> INGREDIENT SCORED (tag-based): {other.gameObject.name}</color>");
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

    private void PlayTrashEffects()
    {
        
        if (splashEffect != null)
        {
            splashEffect.Play();
        }

       
        if (trashSound != null)
        {
            trashSound.Play();
        }
        else if (scoreSound != null)
        {
           
            scoreSound.Play();
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