using UnityEngine;
using System.Collections.Generic;

public class GarbanzoBeanSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int totalBeans = 15;
    [SerializeField] private float spawnRadius = 2f;

    [Header("References")]
    [SerializeField] private GameObject beanPrefab;

    private List<GameObject> spawnedBeans = new List<GameObject>();

    private void Start()
    {
        Debug.Log($"<color=cyan>=== GARBANZO BEAN SPAWNER START ===</color>");
        Debug.Log($"Total beans to spawn: {totalBeans}");
        Debug.Log($"Spawn radius: {spawnRadius}");
        Debug.Log($"Bean prefab assigned: {(beanPrefab != null ? beanPrefab.name : "NULL")}");

        SpawnBeans();
    }

    private void Update()
    {
        // DEBUG: Press B to show bean status
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log($"<color=cyan>=== BEAN SPAWNER STATUS ===</color>");
            Debug.Log($"Total beans spawned: {spawnedBeans.Count}");

            int activeCount = 0;
            int destroyedCount = 0;

            foreach (GameObject bean in spawnedBeans)
            {
                if (bean != null)
                {
                    activeCount++;
                    GarbanzoBean beanScript = bean.GetComponent<GarbanzoBean>();
                    if (beanScript != null)
                    {
                        Debug.Log($"  Bean: {bean.name} at {bean.transform.position}");
                    }
                }
                else
                {
                    destroyedCount++;
                }
            }

            Debug.Log($"Active beans: {activeCount}");
            Debug.Log($"Destroyed beans: {destroyedCount}");
        }
    }

    private void SpawnBeans()
    {
        if (beanPrefab == null)
        {
            Debug.LogError("<color=red> Bean prefab is NULL! Cannot spawn beans!</color>");
            return;
        }

        Debug.Log($"<color=yellow>Starting to spawn {totalBeans} beans...</color>");

        // Spawn beans in a circle around this spawner
        for (int i = 0; i < totalBeans; i++)
        {
            float angle = 2 * Mathf.PI * i / totalBeans;
            Vector3 offset = new Vector3(
                spawnRadius * Mathf.Cos(angle),
                0,
                spawnRadius * Mathf.Sin(angle)
            );

            Vector3 spawnPos = transform.position + offset;
            GameObject bean = Instantiate(beanPrefab, spawnPos, Quaternion.identity);
            bean.name = $"GarbanzoBean_{i}";
            spawnedBeans.Add(bean);

            Debug.Log($"  Spawned bean {i + 1}/{totalBeans} at {spawnPos}");
        }

        Debug.Log($"<color=green>✓ Successfully spawned {totalBeans} garbanzo beans!</color>");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Draw spawn positions
        for (int i = 0; i < totalBeans; i++)
        {
            float angle = 2 * Mathf.PI * i / totalBeans;
            Vector3 offset = new Vector3(
                spawnRadius * Mathf.Cos(angle),
                0,
                spawnRadius * Mathf.Sin(angle)
            );
            Vector3 spawnPos = transform.position + offset;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(spawnPos, 0.1f);
        }
    }
}
