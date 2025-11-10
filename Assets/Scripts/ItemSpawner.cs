using System;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Ingredients Prefabs")]
    [SerializeField] private GameObject[] ingredients;
    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPoint;

    [Header("Number of Ingredients to Spawn")]
    [SerializeField] private int numberToSpawn = 1;
    private Projectile projectile;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void spawnitem()
    {
        if (projectile.currentHeldEmber == null & Input.GetKeyUp(KeyCode.J))
        {
            RandomIngredientSpawn();
        }
        int spawnCount = Mathf.Min(numberToSpawn, ingredients.Length);
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject ingredientPrefab = ingredients[i];
            


            GameObject newIngredient = Instantiate(ingredientPrefab, spawnPoint.position, spawnPoint.rotation);



            Debug.Log($"Spawned {newIngredient.name} at {spawnPoint.name}");
        }
    }

    private void RandomIngredientSpawn()
    {
        ShuffleArray(ingredients);
    }
    //when an ember or ingredeint is thrown it randomly selects an ingredeint or ember to spawn in
    private void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randIndex = UnityEngine.Random.Range(0, i + 1);
            (array[i], array[randIndex]) = (array[randIndex], array[i]);
        }
    }

}
