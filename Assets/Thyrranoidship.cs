using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThyrranoidArcShip : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Transform Point1;
    public Transform Point2;
    public GameObject BulletPrefab;
    public float ShootForce = 70f;
    public float fireRate = 0.25f;           // Tid mellan varje skott (i sekunder)
    public float burstPause = 1.5f;           // Paus mellan skjut-salvor
    public int shotsPerBurst = 6;            // Hur många skott per salva

    [Header("Audio & FX")]
    public AudioClip shootSound;
    public GameObject muzzleFlashPrefab;

    [Header("Flight - Player (Flying)")]
    public float playerOrbitRadius = 30f;
    public float playerFlyHeight = 2f;

    [Header("Flight - Rangers (Ground)")]
    public float rangerOrbitRadius = 15f;
    public float rangerFlyHeight = 10f;

    [Header("Global Flight Settings")]
    public float moveSpeed = 20f;
    public float rotationSpeed = 3.0f;
    public float orbitSpeedMultiplier = 0.5f;
    public float bankingAmount = 55f;

    [Header("AI Logic")]
    public float detectionRange = 60f;
    public LayerMask playerLayer;
    public LayerMask rangerLayer;

    private GameObject currentTarget;
    private bool hasTarget = false;
    private bool targetIsPlayer = false;
    private float orbitTimer;
    private Animator anime;
    private AudioSource audioSource;

    // Skjut-timers
    private float nextFireTime;
    private int currentBurstCount;
    private bool isPauseBetweenBursts = false;

    void Start()
    {
        anime = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        orbitTimer = Random.Range(0f, 10f);
    }

    void Update()
    {
        ManageTargeting();

        if (hasTarget && currentTarget != null)
        {
            HandleCombatFlight();
            HandleShootingTimer(); // Hanterar automatisk skjutning
        }
        else
        {
            // Flyg rakt fram när inget mål finns
            transform.position += transform.forward * (moveSpeed * 0.4f) * Time.deltaTime;
            if (anime) anime.SetBool("IsShooting", false);
        }
    }

    void ManageTargeting()
    {
        if (hasTarget && currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (!currentTarget.activeInHierarchy || dist > detectionRange + 15f)
            {
                hasTarget = false;
                currentTarget = null;
            }
            else return;
        }

        // Prioritera Spelaren
        Collider[] players = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        if (players.Length > 0)
        {
            currentTarget = players[0].gameObject;
            hasTarget = true;
            targetIsPlayer = true;
            return;
        }

        // Annars Rangers
        Collider[] rangers = Physics.OverlapSphere(transform.position, detectionRange, rangerLayer);
        if (rangers.Length > 0)
        {
            currentTarget = rangers[0].gameObject;
            hasTarget = true;
            targetIsPlayer = false;
        }
    }

    void HandleCombatFlight()
    {
        float currentRadius = targetIsPlayer ? playerOrbitRadius : rangerOrbitRadius;
        float currentHeight = targetIsPlayer ? playerFlyHeight : rangerFlyHeight;

        orbitTimer += Time.deltaTime * orbitSpeedMultiplier;

        float x = Mathf.Cos(orbitTimer) * currentRadius;
        float z = Mathf.Sin(orbitTimer) * currentRadius;

        Vector3 targetPos = currentTarget.transform.position + new Vector3(x, currentHeight, z);

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        Vector3 dirToTarget = (currentTarget.transform.position - transform.position).normalized;

        if (dirToTarget != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
            float angleDiff = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);
            float bank = Mathf.Clamp(angleDiff * 3f, -bankingAmount, bankingAmount);

            targetRot *= Quaternion.Euler(0, 0, -bank);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }

    void HandleShootingTimer()
    {
        if (Time.time < nextFireTime) return;

        // Skjut en salva
        Shooting();
        currentBurstCount++;

        if (currentBurstCount >= shotsPerBurst)
        {
            // Ta en paus mellan salvorna
            currentBurstCount = 0;
            nextFireTime = Time.time + burstPause;
            if (anime) anime.SetBool("IsShooting", false);
        }
        else
        {
            // Nästa skott i samma salva
            nextFireTime = Time.time + fireRate;
            if (anime) anime.SetBool("IsShooting", true);
        }
    }

    public void Shooting()
    {
        if (Point1) Fire(Point1);
        if (Point2) Fire(Point2);

        if (shootSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    void Fire(Transform p)
    {
        if (BulletPrefab == null || p == null) return;

        GameObject b = Instantiate(BulletPrefab, p.position, p.rotation);
        
        Rigidbody rb = b.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = p.forward * ShootForce;
        }

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, p.position, p.rotation);
            Destroy(flash, 0.5f);
        }

        Destroy(b, 4f);
    }
}