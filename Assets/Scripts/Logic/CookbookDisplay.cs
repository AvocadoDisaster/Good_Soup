using UnityEngine;
using System.Collections.Generic;

public class CookbookDisplay : MonoBehaviour
{
    [System.Serializable]

    public class IngredientSlot
    {
        public string ingredientName;
        public GameObject unlockedImage;
    }

    public List<IngredientSlot> ingredientSlots = new List<IngredientSlot>();

    private HashSet<string> collectedIngredients = new HashSet<string>();

    public void UnlockIngredient(string ingredientName)
    {
        if (collectedIngredients.Contains(ingredientName))
            return;

        collectedIngredients.Add(ingredientName);

        foreach (var slot in ingredientSlots)
        {
            if (slot.ingredientName == ingredientName && slot.unlockedImage != null)
            {
                slot.unlockedImage.SetActive(true);
                Debug.Log($"Unlocked ingredient: {ingredientName}");
                break;
            }
        }
    }

}
