using UnityEngine;

public class StoneFalling : MonoBehaviour
{
    [Header("Prefabs & Spawn Points")]
    [Tooltip("Dra in din sten-prefab här.")]
    public GameObject stonePrefab;

    [Tooltip("Dra in dina 3 spawnpoints här.")]
    public Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [Tooltip("Tid i sekunder mellan varje ny sten.")]
    public float spawnInterval = 1.2f;

    [Tooltip("Hur många sekunder stenen lever innan den tas bort ur minnet.")]
    public float stoneLifetime = 8f;

    [Header("Physics & Bounce Forces")]
    [Tooltip("Kraft framåt längs gången (mot spelaren).")]
    public float forwardForce = 12f;

    [Tooltip("Kraft nedåt så att stenen slår hårt i marken och studsar.")]
    public float downwardForce = 5f;

    [Tooltip("Slumpmässig spinn/rotation i luften när stenen skapas.")]
    public float torqueAmount = 15f;

    private float timer;

    void Start()
    {
        // Starta timern så att första stenen kommer efter angivet intervall
        timer = spawnInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnStone();
            timer = spawnInterval; // Återställ timern
        }
    }

    void SpawnStone()
    {
        // Säkerhetskontroll så att inget saknas i Inspector
        if (stonePrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("StoneFalling: Glöm inte att dra in 'Stone Prefab' och dina 'Spawn Points' i Inspector!");
            return;
        }

        // 1. Välj en av de 3 spawn-punkterna slumpmässigt
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedSpawn = spawnPoints[randomIndex];

        // 2. Skapa stenen på den valda punkten med dess rotation
        GameObject newStone = Instantiate(stonePrefab, selectedSpawn.position, selectedSpawn.rotation);

        // 3. Lägg till fysikkrafter om stenen har en Rigidbody
        if (newStone.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            // Beräkna kraftriktning (framåt + nedåt)
            Vector3 pushDirection = (selectedSpawn.forward * forwardForce) + (Vector3.down * downwardForce);
            rb.AddForce(pushDirection, ForceMode.Impulse);

            // Ge stenen slumpmässig spinn i alla tre axlar (X, Y, Z)
            Vector3 randomTorque = new Vector3(
                Random.Range(-torqueAmount, torqueAmount),
                Random.Range(-torqueAmount, torqueAmount),
                Random.Range(-torqueAmount, torqueAmount)
            );
            rb.AddForce(pushDirection, ForceMode.VelocityChange);
        }

        // 4. Ta bort stenen efter några sekunder för att hålla spelet snabbt och städat
        Destroy(newStone, stoneLifetime);
    }
}