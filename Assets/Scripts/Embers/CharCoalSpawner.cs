using System.Collections.Generic;
using UnityEngine;

public class CharCoalSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int totalCharcoals = 15;
    [SerializeField] private float spawnRadius = 2f;

    [Header("References")]
    [SerializeField] private GameObject beanPrefab;

    private List<GameObject> spawnedCharcoals = new List<GameObject>();

    private void Start()
    {
        Debug.Log($"<color=cyan>=== Charcoal SPAWNER START ===</color>");
        Debug.Log($"Total beans to spawn: {totalCharcoals}");
        Debug.Log($"Spawn radius: {spawnRadius}");
        Debug.Log($"Charcoal prefab assigned: {(beanPrefab != null ? beanPrefab.name : "NULL")}");

        SpawnCharcoals();
    }

    private void Update()
    {
        // DEBUG: Press L to show bean status
        if (Input.GetKeyDown(KeyCode.L)) 
            {
                Debug.Log($"<color=cyan>=== Charcoal STATUS==</color>");
                Debug.Log($"Total beans spawned: {spawnedCharcoals.Count}");

                int activeCount = 0;
                int destroyedCount = 0;

                foreach (GameObject bean in spawnedCharcoals)
                {
                    if (bean != null)
                    {
                        activeCount++;
                        Charcoal beanScript = bean.GetComponent<Charcoal>();
                        if (beanScript != null)
                        {
                            Debug.Log($"  Charcoal: {bean.name} at {bean.transform.position}");
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
    

    private void SpawnCharcoals()
    {
        if (beanPrefab == null)
        {
            Debug.LogError("<color=red> Charcoal prefab is NULL! Cannot spawn beans!</color>");
            return;
        }

        Debug.Log($"<color=yellow>Starting to spawn {totalCharcoals} beans...</color>");

        // Spawn beans in a circle around this spawner
        for (int i = 0; i < totalCharcoals; i++)
        {
            float angle = 2 * Mathf.PI * i / totalCharcoals;
            Vector3 offset = new Vector3(
                spawnRadius * Mathf.Cos(angle),
                0,
                spawnRadius * Mathf.Sin(angle)
            );

            Vector3 spawnPos = transform.position + offset;
            GameObject bean = Instantiate(beanPrefab, spawnPos, Quaternion.identity);
            bean.name = $"GarbanzoCharcoal_{i}";
            spawnedCharcoals.Add(bean);

            Debug.Log($"  Spawned bean {i + 1}/{totalCharcoals} at {spawnPos}");
        }

        Debug.Log($"<color=green>✓ Successfully spawned {totalCharcoals} garbanzo beans!</color>");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Draw spawn positions
        for (int i = 0; i < totalCharcoals; i++)
        {
            float angle = 2 * Mathf.PI * i / totalCharcoals;
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
