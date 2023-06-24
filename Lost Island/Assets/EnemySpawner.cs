using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject player;
    public float spawnRadius;
    public List<GameObject> enemyPrefabs;
    public LayerMask whatIsGround;

    HiveMind hive;

    void Start()
    {
        //Get HiveMind
        hive = GameObject.FindGameObjectWithTag("ScriptHolder").GetComponent<HiveMind>();

        // Check if player reference is assigned
        if (player == null)
        {
            Debug.LogError("Player reference not assigned!");
        }

        // Check if enemy prefabs list is not empty
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("Enemy prefabs list is empty!");
        }
    }

    void FixedUpdate()
    {
        // Check if enemy needs to be spawned
        if (ShouldSpawnEnemy())
        {
            SpawnEnemy();
        }
    }

    bool ShouldSpawnEnemy()
    {/*
        // Check if player is within spawn radius
        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        if (distanceToPlayer > spawnRadius)
        {
            return false;
        }*/

        // Randomly determine if enemy should be spawned
        float spawnChance = Random.Range(0f, 1f);
        if (spawnChance > 0.98f)
        {
            if(hive.worldEnemies.Count < 4)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    void SpawnEnemy()
    {
        // Generate a random position within spawn radius
        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        Vector3 spawnPosition = player.transform.position + randomOffset;
        spawnPosition.y = player.transform.position.y; // Set the Y position to be the same as the player's Y position initially

        // Raycast down from the spawn position to find the ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition, Vector3.down, out hit, Mathf.Infinity, whatIsGround))
        {
            // Set the Y position to be the hit point's Y position
            spawnPosition.y = hit.point.y;
        }
        else
        {
            Debug.LogWarning("Enemy could not find ground to spawn on!");
        }

        // Select a random enemy prefab from the list
        int randomIndex = Random.Range(0, enemyPrefabs.Count);
        GameObject enemyPrefab = enemyPrefabs[randomIndex];

        // Spawn enemy prefab at the generated position
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Enemy spawned");

        hive.AddEnemy(enemy);
    }
}
