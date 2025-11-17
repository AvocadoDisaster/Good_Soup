using UnityEngine;
using System.Collections;

public class ItemSpawner : MonoBehaviour
{
    [Header("Ingredients Prefabs")]
    [SerializeField] private Projectile[] ingredients;

    [Header("Spawn Point")]
    [SerializeField] private Transform spawnPoint;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDelay = 0.3f;

    private int lastIndex = -1;
    private bool canSpawn = true;

    [SerializeField] private Transform spoon;

    private void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        SpawnIngredient();
    }

    public void SpawnIngredient()
    {
        if (!canSpawn || ingredients.Length == 0) return;
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        canSpawn = false;
        yield return new WaitForSeconds(spawnDelay);

        int index = GetRandomIngredientIndex();
        Projectile prefab = ingredients[index];

        Projectile instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        instance.SetSpoon(spoon); // IMPORTANT FIX

        canSpawn = true;
    }

    private int GetRandomIngredientIndex()
    {
        if (ingredients.Length == 1) return 0;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, ingredients.Length);
        }
        while (randomIndex == lastIndex);

        lastIndex = randomIndex;
        return randomIndex;
    }
}
