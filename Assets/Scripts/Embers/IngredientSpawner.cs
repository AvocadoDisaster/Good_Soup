using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngredientSpawner : MonoBehaviour
{
    [Header("Ingredient Prefabs")]
    [SerializeField] private List<IngredientPrefabData> ingredientPrefabs = new List<IngredientPrefabData>();

    [Header("Spawn Locations")]
    [SerializeField] private Transform[] spawnLocations = new Transform[12];
    [SerializeField] private bool useCustomPositions = false;
    [SerializeField] private Vector3[] customPositions = new Vector3[12];

    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool randomizeLocations = true; // Shuffle which ingredient goes where
    [SerializeField] private float spawnHeight = 0.5f; // Height above ground

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color gizmoColor = Color.green;

    private List<GameObject> spawnedIngredients = new List<GameObject>();
   [SerializeField] private PotController potController;

    [System.Serializable]
    public class IngredientPrefabData
    {
        public string ingredientName;
        public GameObject prefab;
    }

    private void Start()
    {
        // Find pot controller to get required ingredients
        

        if (potController == null)
        {
            Debug.LogError("<color=red>PotController not found! Cannot determine which ingredients to spawn.</color>");
            return;
        }

        // Validate setup
        if (!ValidateSetup())
        {
            return;
        }

        if (spawnOnStart)
        {
            SpawnIngredients();
        }
    }

    private bool ValidateSetup()
    {
        Debug.Log("<color=cyan>=== INGREDIENT SPAWNER VALIDATION ===</color>");

        // Check if we have spawn locations
        bool hasLocations = false;

        if (useCustomPositions)
        {
            if (customPositions == null || customPositions.Length < 12)
            {
                Debug.LogError("<color=red>Custom Positions array must have 12 positions!</color>");
                return false;
            }
            hasLocations = true;
            Debug.Log("<color=green>✓ Using 12 custom positions</color>");
        }
        else
        {
            int validLocations = 0;
            for (int i = 0; i < spawnLocations.Length; i++)
            {
                if (spawnLocations[i] != null)
                {
                    validLocations++;
                }
            }

            if (validLocations < 12)
            {
                Debug.LogError($"<color=red>Only {validLocations}/12 spawn locations assigned! Need all 12.</color>");
                return false;
            }
            hasLocations = true;
            Debug.Log($"<color=green>✓ All 12 spawn location transforms assigned</color>");
        }

        // Check if we have ingredient prefabs
        if (ingredientPrefabs == null || ingredientPrefabs.Count == 0)
        {
            Debug.LogError("<color=red>No ingredient prefabs assigned!</color>");
            return false;
        }

        Debug.Log($"<color=green>✓ {ingredientPrefabs.Count} ingredient prefabs available</color>");

        // Validate each prefab
        int validPrefabs = 0;
        foreach (var data in ingredientPrefabs)
        {
            if (data.prefab != null && !string.IsNullOrEmpty(data.ingredientName))
            {
                validPrefabs++;
            }
            else
            {
                Debug.LogWarning($"<color=yellow>Invalid prefab data: {data.ingredientName} - prefab is null or name is empty</color>");
            }
        }

        Debug.Log($"<color=cyan>Valid prefabs: {validPrefabs}</color>");
        return true;
    }

    public void SpawnIngredients()
    {
        Debug.Log("<color=cyan>=== SPAWNING INGREDIENTS ===</color>");

        // Clear any existing spawned ingredients
        ClearSpawnedIngredients();

        if (potController == null)
        {
            Debug.LogError("<color=red>PotController not found!</color>");
            return;
        }

        // Get the list of all possible ingredients from PotController
        List<string> allIngredients = potController.allPossibleIngredients;

        // Add Meat and Garbonzo (these are always in the game)
        List<string> ingredientsToSpawn = new List<string>();
        ingredientsToSpawn.Add("Meat");
        ingredientsToSpawn.AddRange(allIngredients);
        // Note: We don't spawn individual Garbonzo beans here - they have their own spawner

        Debug.Log($"<color=cyan>Ingredients to spawn: {string.Join(", ", ingredientsToSpawn)}</color>");

        // Make sure we have exactly 12 ingredients
        if (ingredientsToSpawn.Count > 12)
        {
            Debug.LogWarning($"<color=yellow>Too many ingredients ({ingredientsToSpawn.Count}), using first 12</color>");
            ingredientsToSpawn = ingredientsToSpawn.Take(12).ToList();
        }
        else if (ingredientsToSpawn.Count < 12)
        {
            Debug.LogError($"<color=red>Not enough ingredients ({ingredientsToSpawn.Count}/12)!</color>");
            return;
        }

        // Shuffle ingredients if randomize is enabled
        if (randomizeLocations)
        {
            ingredientsToSpawn = ingredientsToSpawn.OrderBy(x => Random.value).ToList();
            Debug.Log("<color=cyan>Ingredients shuffled!</color>");
        }

        // Spawn each ingredient at its location
        for (int i = 0; i < 12; i++)
        {
            string ingredientName = ingredientsToSpawn[i];
            Vector3 spawnPosition = GetSpawnPosition(i);

            // Find the prefab for this ingredient
            IngredientPrefabData prefabData = ingredientPrefabs.Find(x => x.ingredientName == ingredientName);

            if (prefabData == null || prefabData.prefab == null)
            {
                Debug.LogError($"<color=red>No prefab found for ingredient: {ingredientName}</color>");
                continue;
            }

            // Spawn the ingredient
            GameObject spawned = Instantiate(prefabData.prefab, spawnPosition, Quaternion.identity);
            spawned.name = $"{ingredientName}_Spawned";
            spawnedIngredients.Add(spawned);

            Debug.Log($"<color=green>✓ Spawned {ingredientName} at location {i + 1}: {spawnPosition}</color>");
        }

        Debug.Log($"<color=green>✓✓✓ Successfully spawned {spawnedIngredients.Count} ingredients!</color>");
    }

    private Vector3 GetSpawnPosition(int index)
    {
        if (useCustomPositions)
        {
            Vector3 pos = customPositions[index];
            pos.y += spawnHeight;
            return pos;
        }
        else
        {
            Vector3 pos = spawnLocations[index].position;
            pos.y += spawnHeight;
            return pos;
        }
    }

    public void ClearSpawnedIngredients()
    {
        Debug.Log("<color=yellow>Clearing spawned ingredients...</color>");

        foreach (GameObject ingredient in spawnedIngredients)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }

        spawnedIngredients.Clear();
        Debug.Log("<color=yellow>✓ All spawned ingredients cleared</color>");
    }

    public void RespawnIngredients()
    {
        ClearSpawnedIngredients();
        SpawnIngredients();
    }

    // Automatically create 12 spawn locations in a grid pattern
    [ContextMenu("Create Default Spawn Locations")]
    private void CreateDefaultSpawnLocations()
    {
        // Clear existing spawn locations parent
        Transform existingParent = transform.Find("SpawnLocations");
        if (existingParent != null)
        {
            DestroyImmediate(existingParent.gameObject);
        }

        // Create new parent
        GameObject parent = new GameObject("SpawnLocations");
        parent.transform.SetParent(transform);
        parent.transform.localPosition = Vector3.zero;

        // Create 12 locations in a 4x3 grid
        int index = 0;
        float spacing = 5f;

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                GameObject locObj = new GameObject($"SpawnLocation_{index + 1}");
                locObj.transform.SetParent(parent.transform);

                Vector3 position = new Vector3(
                    col * spacing - (spacing * 1.5f), // Center the grid
                    0,
                    row * spacing - spacing
                );

                locObj.transform.position = transform.position + position;
                spawnLocations[index] = locObj.transform;

                index++;
            }
        }

        Debug.Log("<color=green>✓ Created 12 default spawn locations in a 4x3 grid</color>");
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Gizmos.color = gizmoColor;

        // Draw spawn locations
        for (int i = 0; i < 12; i++)
        {
            Vector3 position = Vector3.zero;
            bool hasPosition = false;

            if (useCustomPositions && customPositions != null && i < customPositions.Length)
            {
                position = customPositions[i];
                position.y += spawnHeight;
                hasPosition = true;
            }
            else if (!useCustomPositions && spawnLocations != null && i < spawnLocations.Length && spawnLocations[i] != null)
            {
                position = spawnLocations[i].position;
                position.y += spawnHeight;
                hasPosition = true;
            }

            if (hasPosition)
            {
                // Draw sphere at spawn point
                Gizmos.DrawWireSphere(position, 0.5f);

                // Draw line down to ground
                Gizmos.DrawLine(position, position - Vector3.up * spawnHeight);

#if UNITY_EDITOR
                // Draw location number
                UnityEditor.Handles.Label(position + Vector3.up * 0.5f, $"{i + 1}");
#endif
            }
        }

        // Draw grid lines
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        for (int i = 0; i < 11; i++)
        {
            Vector3 pos1 = Vector3.zero;
            Vector3 pos2 = Vector3.zero;
            bool canDraw = false;

            if (useCustomPositions && customPositions != null && i < customPositions.Length)
            {
                pos1 = customPositions[i];
                pos2 = customPositions[i + 1];
                canDraw = true;
            }
            else if (!useCustomPositions && spawnLocations != null && i < spawnLocations.Length &&
                     spawnLocations[i] != null && spawnLocations[i + 1] != null)
            {
                pos1 = spawnLocations[i].position;
                pos2 = spawnLocations[i + 1].position;
                canDraw = true;
            }

            if (canDraw)
            {
                Gizmos.DrawLine(pos1, pos2);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Draw larger spheres when selected
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 12; i++)
        {
            Vector3 position = Vector3.zero;
            bool hasPosition = false;

            if (useCustomPositions && customPositions != null && i < customPositions.Length)
            {
                position = customPositions[i];
                position.y += spawnHeight;
                hasPosition = true;
            }
            else if (!useCustomPositions && spawnLocations != null && i < spawnLocations.Length && spawnLocations[i] != null)
            {
                position = spawnLocations[i].position;
                position.y += spawnHeight;
                hasPosition = true;
            }

            if (hasPosition)
            {
                Gizmos.DrawWireSphere(position, 0.7f);
            }
        }
    }
}