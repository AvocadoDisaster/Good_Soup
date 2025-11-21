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
    [SerializeField] private float spawnHeight = 0.1f; // Small offset above spawn point

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

        // Spawn slightly above the spawn point to prevent ground collision
        Vector3 spawnPosition = spawnPoint.position + Vector3.up * spawnHeight;

        Projectile instance = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // CRITICAL: Setup physics IMMEDIATELY after instantiation
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Stop all physics first
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable collider to prevent unwanted collisions
        Collider col = instance.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Set the spoon reference
        instance.SetSpoon(spoon);

        // Set spawner reference
        instance.spawner = this;

        Debug.Log($"<color=green>Spawned {instance.name} at {spawnPosition}</color>");

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