using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Droid : MonoBehaviour
{
    [Header("Referenser")]
    public GameObject Particle;
    public Transform point;
    public Animator[] animes;
    private GameObject Player;

    [Header("Strid & Detektion")]
    public float DetectionRange = 15f;
    public float StopDistance = 7f;    // --- EXAKT POSITION ---
    public float ChaseSpeed = 8f;      // Högre fart kräver bättre stopp-logik
    public float ShootTime = 1.0f;     // Snabbare salvor för Zeldrin-stilen
    public float ShootForce = 20f;
    private float shootTimer;
    private bool sePlayer = false;
    private bool hasReachedShootingStation = false;

    [Header("Patrullering")]
    public float WalkSpeed = 3f;
    public float PatrolTime = 4f;
    public float Idletime = 2f;
    public float detectionDistance = 1.5f;
    public float RotationSpeed = 100f;

    private float currentPatrolTimer;
    private float currentIdleTimer;
    private bool isPatrolling = true;

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        currentPatrolTimer = PatrolTime;
        currentIdleTimer = Idletime;
        shootTimer = ShootTime;
    }

    void Update()
    {
        // Vi avslutar direkt om droiden redan stannat och står och skjuter
        if (hasReachedShootingStation)
        {
            HandleCombatStationary();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, Player.transform.position);

        // 1. Kolla om vi ser spelaren
        if (distanceToPlayer < DetectionRange)
        {
            sePlayer = true;
        }
        else
        {
            sePlayer = false;
        }

        // 2. AI-Tillstånd
        if (sePlayer)
        {
            HandleCombatChase(distanceToPlayer);
        }
        else
        {
            HandlePatrol();
        }

        UpdateAnimations();
    }

    void HandlePatrol()
    {
        if (isPatrolling)
        {
            transform.Translate(Vector3.forward * WalkSpeed * Time.deltaTime);

            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, detectionDistance))
            {
                if (!hit.collider.CompareTag("Player"))
                {
                    StopAndTurn();
                }
            }

            currentPatrolTimer -= Time.deltaTime;
            if (currentPatrolTimer <= 0)
            {
                isPatrolling = false;
                currentIdleTimer = Idletime;
            }
        }
        else
        {
            transform.Rotate(0, RotationSpeed * Time.deltaTime, 0);

            currentIdleTimer -= Time.deltaTime;
            if (currentIdleTimer <= 0)
            {
                isPatrolling = true;
                currentPatrolTimer = PatrolTime;
            }
        }
    }

    void HandleCombatChase(float distance)
    {
        // Titta på spelaren
        LookAtPlayer();

        // --- EXAKT POSITION (STOPP-LOGIK) ---
        // Istället för en hård if-sats, använder vi en liten marginal.
        // Om avståndet är mindre än StopDistance plus en liten buffert
        if (distance <= StopDistance + 0.2f)
        {
            // Vi är framme! Stanna för gott.
            hasReachedShootingStation = true;
            sePlayer = false; // Bryt "sePlayer"-animationen
            return;
        }

        // Annars: Jaga!
        // Använd transform.MoveTowards istället för transform.Translate
        // för att förhindra att den "glider" förbi StopDistance.
        Vector3 targetPos = Player.transform.position;
        targetPos.y = transform.position.y; // Håll droiden på marken
        transform.position = Vector3.MoveTowards(transform.position, targetPos, ChaseSpeed * Time.deltaTime);
    }

    void HandleCombatStationary()
    {
        // Titta på spelaren
        LookAtPlayer();

        // Stå stilla och skjut
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            foreach (Animator anim in animes)
            {
                anim.SetTrigger("Shoot");
            }
            shootTimer = ShootTime;
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = Player.transform.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 15f);
        }
    }

    void UpdateAnimations()
    {
        foreach (Animator anim in animes)
        {
            // --- RUN ANIMATION ---
            // Om vi ser spelaren och inte har stannat ännu, så springer vi.
            anim.SetBool("Run", sePlayer && !hasReachedShootingStation);

            // Gå-animationen styrs av patrulleringen
            anim.SetBool("Walk", isPatrolling && !sePlayer && !hasReachedShootingStation);

            // "SePlayer" (Sikta) animationen
            anim.SetBool("SePlayer", sePlayer || hasReachedShootingStation);
        }
    }

    void StopAndTurn()
    {
        isPatrolling = false;
        currentIdleTimer = 1.0f;
    }

    public void Shoot()
    {
        if (Particle != null && point != null)
        {
            GameObject prefab = Instantiate(Particle, point.position, point.rotation);
            Rigidbody rb = prefab.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = point.forward * ShootForce;
            Destroy(prefab, 5f);
        }
    }
}