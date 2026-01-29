using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorEnemies : MonoBehaviour
{
    public Transform[] enemies;       // Array of enemy prefabs to spawn
    public Transform[] spawnPoints;   // Array of spawn points

    private List<Transform> EnemiesSpawned = new List<Transform>(); // List of spawned enemies
    private bool isSpawn = false; // Flag to track if spawning is in progress
    Animator anime;
    public int GladiatorFight;
    public int MaxGladiator;
    private int currentEnemySpawnCount = 0;
    public int EnemiesTimeSpawn;
    int StartSpawn;
    // Start is called before the first frame update
    void Start()
    {
        // Initialize list
        GameObject.FindObjectOfType<RoundCount>().roundcount.SetActive(true);
        GameObject.FindObjectOfType<RoundCount>().MaxCount = MaxGladiator;
        
        anime = GetComponent<Animator>();
        StartCoroutine(WaitToSpawn());
        //EnemiesSpawned.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if there are no spawned enemies and spawning is not already happening


        if(MaxGladiator > GladiatorFight)
        {
            if (EnemiesSpawned.Count == 0 && isSpawn == false)
            {
                
                StartCoroutine(WaitToSpawn());
                // Prevent multiple coroutines from starting
            }

            // Clean up list by removing any destroyed enemies
            EnemiesSpawned.RemoveAll(enemy => enemy == null);
        }

       
    }

    // Coroutine to wait and then spawn enemies
    IEnumerator WaitToSpawn()
    {
        anime.SetBool("Trigger", true);
        yield return new WaitForSeconds(1); // Wait for 2 seconds
        if(isSpawn == false)
        {
            SpawnEnemie();
            if(MaxGladiator > GladiatorFight)
            {
                GladiatorFight++;// Spawn enemies
                GameObject.FindObjectOfType<RoundCount>().RoundCount_ = GladiatorFight;

            }
            isSpawn = true;
        }
                       // Reset the flag after spawning
       
        yield return new WaitForSeconds(2);
        isSpawn = false;
        anime.SetBool("Trigger", false);
    }

    // Spawn enemies in a round-robin fashion across spawn points
    public void SpawnEnemie()
    {
        int spawnIndex = 0; // Index to track the next spawn point

        // Spawn the current enemy `EnemiesTimeSpawn` times
        for (int i = 0; i < EnemiesTimeSpawn; i++)
        {
            // Spawn the enemy at the current spawn point
            Transform ins = Instantiate(enemies[StartSpawn], spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);

            // Add spawned enemy to the list
            EnemiesSpawned.Add(ins);

            // Move to the next spawn point
            spawnIndex++;

            // If we've reached the last spawn point, loop back to the first
            if (spawnIndex >= spawnPoints.Length)
            {
                spawnIndex = 0;
            }
        }

        // Increment the spawn count for the current enemy prefab
        StartSpawn++;

        // If we've reached the end of the enemy list, loop back to the first
        if (StartSpawn >= enemies.Length)
        {
            StartSpawn = 0;
        }
    }

}
