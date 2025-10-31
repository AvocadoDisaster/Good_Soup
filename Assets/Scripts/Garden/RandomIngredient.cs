using System.Collections.Generic;
using UnityEngine;

public class RandomIngredientSpawner : MonoBehaviour
{
    [Header("Pot Controller Script")]
    [SerializeField] private PotController potController;

    [Header("Ingredients Prefabs")]
    [SerializeField] private GameObject[] ingredients;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Number of Ingredients to Spawn")]
    [SerializeField] private int numberToSpawn = 12;

    void Start()
    {
        RandomIngredientSpawn();
    }

    //-----------------------------------------------------------------------------------------------------------------------
    private void RandomIngredientSpawn()
    {
        if (ingredients.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No ingredients or spawn points assigned!");
            return;
        }

        // Clamp spawn count
        int spawnCount = Mathf.Min(numberToSpawn, ingredients.Length, spawnPoints.Length);

        // Shuffle both lists
        ShuffleArray(ingredients);
        ShuffleArray(spawnPoints);

        // Spawn or reposition ingredients
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject ingredientPrefab = ingredients[i];
            Transform spawnPoint = spawnPoints[i];

            // Option 1: Instantiate new objects
            GameObject newIngredient = Instantiate(ingredientPrefab, spawnPoint.position, spawnPoint.rotation);

            // Option 2: If you already have them in scene, just move them:
            // ingredientPrefab.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            Debug.Log($"Spawned {newIngredient.name} at {spawnPoint.name}");
        }
    }

    private void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randIndex = Random.Range(0, i + 1);
            (array[i], array[randIndex]) = (array[randIndex], array[i]);
        }
    }
}
