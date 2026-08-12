using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SpawnTime : MonoBehaviour
{
    [Header("Dropship Settings")]
    public GameObject[] SpawnDropship;
    public List<GameObject> DropshipsSpawned = new List<GameObject>();
    public float TimeSpawnShip = 5f;
    private float startTime;
    public Transform[] SpawnPoint; // Fallback om Spline saknas

    [Header("Splines for Dropships")]
    [Tooltip("Dra in din SplineContainer från Scene-vyn hit för varje våg!")]
    public SplineContainer[] DropshipSplines;

    [Header("Enemy Settings")]
    public GameObject[] SpawnEnemie;
    public List<GameObject> EnemiesSpawned = new List<GameObject>();
    public float TimeSpawnEnemie = 10f;
    private float StartTimeEnemies;
    public Transform SpawnPointEnemie;

    [Header("Audio & Mission")]
    public AudioClip[] MissionSound;
    public AudioSource sound;

    [Header("Current Wave / Index")]
    public int SpawnInt = 0;

    void Start()
    {
        startTime = TimeSpawnShip;
        StartTimeEnemies = TimeSpawnEnemie;

        if (sound == null)
            sound = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Rensa bort förstörda objekt ur listorna
        DropshipsSpawned.RemoveAll(ship => ship == null);
        EnemiesSpawned.RemoveAll(enemy => enemy == null);

        // Timer för Dropships
        if (DropshipsSpawned.Count == 0)
        {
            TimeSpawnShip -= Time.deltaTime;
            if (TimeSpawnShip < 0)
            {
                TimeSpawnShip = startTime;
                Spawn();
            }
        }

        // Timer för Enemie Spawns
        if (EnemiesSpawned.Count == 0)
        {
            TimeSpawnEnemie -= Time.deltaTime;
            if (TimeSpawnEnemie < 0)
            {
                TimeSpawnEnemie = StartTimeEnemies;
                SpawnEnemies();
            }
        }
    }

    public void RangerTalk()
    {
        if (MissionSound != null && SpawnInt < MissionSound.Length)
        {
            sound.clip = MissionSound[SpawnInt];
            sound.Play();
        }

        SpawnInt++;
    }

    void Spawn()
    {
        // Säkerhetskontroll för array-gränser
        if (SpawnDropship == null || SpawnDropship.Length == 0 || SpawnInt >= SpawnDropship.Length)
        {
            Debug.LogWarning($"[SpawnTime] SpawnInt ({SpawnInt}) är utanför index för SpawnDropship!");
            return;
        }

        // Hämta aktiv Spline om den finns i listan
        SplineContainer activeSpline = null;
        if (DropshipSplines != null && SpawnInt < DropshipSplines.Length)
        {
            activeSpline = DropshipSplines[SpawnInt];
        }

        Vector3 spawnPos = DropshipSplines[SpawnInt].Spline.GetBounds().center; // Default position om ingen Spline finns
        Quaternion spawnRot = Quaternion.identity;
        bool validSpawnFound = false;

        if (activeSpline != null)
        {
            // Beräkna startposition från Spline
            Vector3 localPos = (Vector3)activeSpline.EvaluatePosition(0f);
            Vector3 localTangent = (Vector3)activeSpline.EvaluateTangent(0f);

            spawnPos = activeSpline.transform.TransformPoint(localPos);
            Vector3 worldTangent = activeSpline.transform.TransformDirection(localTangent);

            spawnRot = (worldTangent != Vector3.zero) ? Quaternion.LookRotation(worldTangent) : activeSpline.transform.rotation;
            validSpawnFound = true;
        }
        else if (SpawnPoint != null && SpawnPoint.Length > SpawnInt && SpawnPoint[SpawnInt] != null)
        {
            // Fallback till SpawnPoint om ingen Spline finns
            spawnPos = SpawnPoint[SpawnInt].position;
            spawnRot = SpawnPoint[SpawnInt].rotation;
            validSpawnFound = true;
        }

        if (!validSpawnFound)
        {
            Debug.LogError($"[SpawnTime] Varken Spline eller SpawnPoint finns inställd för index {SpawnInt}!");
            return;
        }

        // Instansiera skeppet
        GameObject sp = Instantiate(SpawnDropship[SpawnInt], SpawnPoint[SpawnInt].transform.position, spawnRot);

        // Skicka med splinen till skeppsskriptet
        if (activeSpline != null && sp.TryGetComponent<SmoothDropshipSpline>(out var splineScript))
        {
            splineScript.InitSpline(activeSpline);
        }

        DropshipsSpawned.Add(sp);
    }

    void SpawnEnemies()
    {
        if (SpawnInt >= SpawnEnemie.Length) return;

        GameObject sp = Instantiate(SpawnEnemie[SpawnInt], SpawnPointEnemie.position, SpawnPointEnemie.rotation);
        EnemiesSpawned.Add(sp);
    }
}