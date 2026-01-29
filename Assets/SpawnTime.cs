using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTime : MonoBehaviour
{
    public GameObject[] SpawnDropship;
    public List<GameObject> DropshipsSpawned;
    public float TimeSpawnShip;
    float startTime;
    public Transform[] SpawnPoint;
    public int SpawnInt;
    GameObject enemiesSpawned;
    bool isSpawned = false;
    public GameObject[] SpawnEnemie;
    public List<GameObject> EnemiesSpawned;
    public float TimeSpawnEnemie;
    float StartTimeEnemies;
    public Transform SpawnPointEnemie;
    public AudioClip[] MissionSound;
    public AudioSource sound;
    // Start is called before the first frame update
    void Start()
    {
       
        startTime = TimeSpawnShip;
        StartTimeEnemies = TimeSpawnEnemie;
        sound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {


        

            if (DropshipsSpawned.Count == 0)
            {
                TimeSpawnShip -= Time.deltaTime;
                if (TimeSpawnShip < 0)
                {
                    isSpawned = true;

                    TimeSpawnShip = startTime;

                    if (isSpawned)
                    {

                        isSpawned = false;
                        Spawn();

                    }


                }
            }

        if (EnemiesSpawned.Count == 0)
        {
            TimeSpawnEnemie -= Time.deltaTime;
            if (TimeSpawnEnemie < 0)
            {
                isSpawned = true;

                TimeSpawnEnemie = startTime;

                if (isSpawned)
                {

                    isSpawned = false;
                    SpawnEnemies();

                }


            }
        }


        





    }


    public void RangerTalk()
    {
        SpawnInt++;
        
        sound.clip = MissionSound[SpawnInt];
        sound.Play();
    }


    void Spawn()
    {
        GameObject sp = Instantiate(SpawnDropship[SpawnInt], SpawnPoint[SpawnInt].transform.position, SpawnPoint[SpawnInt].transform.rotation);

        DropshipsSpawned.Add(sp);
       

    }

    void SpawnEnemies()
    {
        GameObject sp = Instantiate(SpawnEnemie[SpawnInt], SpawnPointEnemie.transform.position, SpawnPointEnemie.transform.rotation);

        EnemiesSpawned.Add(sp);
    }
 }
