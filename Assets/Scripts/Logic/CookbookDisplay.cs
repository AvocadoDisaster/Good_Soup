using UnityEngine;
using System.Collections.Generic;

public class CookbookDisplay : MonoBehaviour
{
    //[System.Serializable]

    public PotController pot;

    public class IngredientSlot
    {
        public string ingredientName;
        public GameObject unlockedImage;
    }

    public List<IngredientSlot> ingredientSlots = new List<IngredientSlot>();


    public void UnlockIngredient(string ingredientName)
    {
        for (int i = 0; i < pot.selectedIngredients.Count; i++)
        {
            //pot.selectedIngredients[i];
            
        }

        if (pot.selectedIngredients.Contains(ingredientName))
            return;

        //pot.selectedIngredients;

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
