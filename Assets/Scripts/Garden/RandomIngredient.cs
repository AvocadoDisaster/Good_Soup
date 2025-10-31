using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RandomIngredient : MonoBehaviour
{
    [Header("Pot Controller Script")]
    [SerializeField] public PotController potController;

    [Header("Ingredients")]
    [SerializeField] public GameObject[] ingredients;

    [Header("Spawn Points")]
    [SerializeField] public Transform[] spawnPoints;

    void Start()
    {
        RandomIngredientSpawn();
    }
    

    //-----------------------------------------------------------------------------------------------------------------------
    void RandomIngredientSpawn()
    {
        //Shuffle the position of ingredients
        RandomIngredientIndex(ingredients);

        int ingredientNum = potController.allPossibleIngredients.Count;

        for (int i = 1; i < ingredientNum; i++)
        {
            GameObject ingredientToMove = ingredients[i];

            Transform spawnPoint = spawnPoints[i];

            ingredientToMove.transform.position = spawnPoint.position;
            ingredientToMove.transform.rotation = spawnPoint.rotation;
            
            Debug.Log($"Repositioned {ingredientToMove.name} to {spawnPoint.name}.");
        }
    }

    private void RandomIngredientIndex(GameObject[] array)
    {
        // Iterate backwards through the array
        for (int i = array.Length - 1; i > 1; i--)
        {
            // Pick a random index from 0 to i (inclusive)
            int randIndex = Random.Range(1, i + 1);

            // Swap the current element
            (array[randIndex], array[i]) = (array[i], array[randIndex]);
        }
    }
}