using UnityEngine;

public class PotCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check for ingredient
        if (other.CompareTag("Ingredient"))
        {
            Debug.Log($"<color=green>{other.name} entered the pot!</color>");
            GameManager.Instance.IngredientScored(other.name);
            Destroy(other.gameObject);
        }

        // Check for ember
        else if (other.CompareTag("Ember"))
        {
            Debug.Log($"<color=orange>{other.name} (ember) entered the pot!</color>");
            GameManager.Instance.EmberScored();
            Destroy(other.gameObject);
        }
    }
}
