using UnityEngine;

public class IngredientPickup : MonoBehaviour
{
    public string ingredientName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CookbookDisplay inventory = other.GetComponent<CookbookDisplay>();
            if (inventory != null)
            {
                inventory.UnlockIngredient(ingredientName);
                Destroy(gameObject);
            }
        }
    }

}
