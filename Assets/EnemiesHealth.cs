using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesHealth : MonoBehaviour, IInfectable
{
    [Header("Health Settings")]
    public int health = 1;
    public int maxHealth;
    public bool BossHealth = false;
    public float LevelXp = 0.5f;
    public static EnemiesHealth EnemieHealth_;

    [Header("Visual & Damage Effects")]
    public Color damageColor = Color.red;
    public Color startColor = Color.white;
    public float colorChangeDuration = 0.2f;
    public Renderer[] MaterialRed;
    public Animator anime;
    public Animator animes;
    public Transform ExplodePrefab;
    public bool DamageExplode = false;
    public float ExplodeTime;
    public float DamageHitRange = 5f;

    [Header("Audio")]
    private AudioSource sound;
    public AudioClip clipsound;
    public AudioClip SoundDamage;

    [Header("Drops & Interactions")]
    public GameObject Bolt;

    [Header("Infector & Targeting System")]
    public bool IsInfector = false;
    public LayerMask PlayerDetect;
    [Tooltip("Target for movement/shooting scripts to follow")]
    public Transform currentTarget; 
    public GameObject InfectorEffect;
    [Tooltip("Base duration of infection at Level 1 (seconds)")]
    public float baseInfectionDuration = 15f; 
    [Tooltip("Extra seconds per level above Level 1")]
    public float durationIncreasePerLevel = 5f; 

    [Header("Edge Protection (Non-NavMesh)")]
    [Tooltip("Distance ahead to check for floor")]
    public float edgeCheckDistance = 0.6f;
    [Tooltip("Ground/Bridge LayerMask")]
    public LayerMask groundLayer;

    [Header("Mission Data")]
    public int IndexMission;
    public bool IsMissionEnemy = false;

    // Private Runtime State
    private Rigidbody rb;
    private List<Material> cachedMaterials = new List<Material>();
    private bool damagish = false;
    private float changeColorTimeTimer = 0.2f;
    
    private bool isCurrentlyInfected = false;
    private float targetScanTimer = 0f;
    private float infectionTimer = 0f;
    private float currentInfectionDuration = 15f;
    private GameObject infectorClone;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sound = GetComponent<AudioSource>();

        // Cache all material instances once to prevent memory leaks in Update
        MaterialRed = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in MaterialRed)
        {
            foreach (Material mat in rend.materials)
            {
                cachedMaterials.Add(mat);
            }
        }
    }

    private void Start()
    {
        if (BossHealth)
        {
            EnemieHealth_ = this;
        }

        if (anime == null) anime = GetComponentInChildren<Animator>();
        maxHealth = health;

        StartCoroutine(LaserInitializationWait());
        FindTarget();
    }

    private void FixedUpdate()
    {
        HandleEdgeProtection();
    }

    private void Update()
    {
        HandleDamageColorBlink();
        HandleInfectionTimer();

        // Throttle target scanning (5 times per second)
        targetScanTimer += Time.deltaTime;
        if (targetScanTimer > 0.2f)
        {
            targetScanTimer = 0f;
            FindTarget();
        }
    }

    // --- EDGE PROTECTION ---
    private void HandleEdgeProtection()
    {
        if (rb == null) return;

        Vector3 moveDirection = rb.linearVelocity;
        moveDirection.y = 0;

        if (moveDirection.magnitude > 0.05f)
        {
            Vector3 normalizedDir = moveDirection.normalized;
            Vector3 checkPosition = transform.position + (normalizedDir * edgeCheckDistance);
            Vector3 rayOrigin = checkPosition + Vector3.up * 1.0f;

            if (!Physics.Raycast(rayOrigin, Vector3.down, 1.5f, groundLayer))
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                rb.position -= normalizedDir * 0.05f;
            }
        }
    }

    // --- VISUAL EFFECTS ---
    private void HandleDamageColorBlink()
    {
        if (!damagish) return;

        changeColorTimeTimer -= Time.deltaTime;
        Color targetColor = (changeColorTimeTimer < 0) ? startColor : damageColor;

        foreach (Material mat in cachedMaterials)
        {
            mat.color = targetColor;
        }

        if (changeColorTimeTimer < 0)
        {
            damagish = false;
            changeColorTimeTimer = colorChangeDuration;
        }
    }

    // --- INFECT SYSTEM ---
    private void HandleInfectionTimer()
    {
        if (!isCurrentlyInfected) return;

        infectionTimer += Time.deltaTime;
        if (infectionTimer >= currentInfectionDuration)
        {
            Infect(false);
            Debug.Log($"{gameObject.name} is no longer infected.");
        }
    }

    public void Infect(bool state) => Infect(state, 1);

    public void Infect(bool state, int level)
    {
        isCurrentlyInfected = state;
        IsInfector = state;

        if (state)
        {
            if (infectorClone == null && InfectorEffect != null)
            {
                infectorClone = Instantiate(InfectorEffect, transform.position, transform.rotation, transform);
                infectorClone.transform.localPosition = Vector3.zero;

                SkinnedMeshRenderer enemyMesh = GetComponentInChildren<SkinnedMeshRenderer>();
                ParticleSystem ps = infectorClone.GetComponent<ParticleSystem>();

                if (enemyMesh != null && ps != null)
                {
                    var shape = ps.shape;
                    shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                    shape.skinnedMeshRenderer = enemyMesh;
                }
            }

            int clampedLevel = Mathf.Clamp(level, 1, 5);
            currentInfectionDuration = baseInfectionDuration + ((clampedLevel - 1) * durationIncreasePerLevel);
            infectionTimer = 0f;
        }
        else
        {
            if (infectorClone != null)
            {
                Destroy(infectorClone);
                infectorClone = null;
            }
            currentTarget = null;
        }

        FindTarget();
    }

    // --- TARGETING LOGIC ---
    private void FindTarget()
    {
        if (isCurrentlyInfected)
        {
            currentTarget = FindNearestOtherEnemy();
        }
        else
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 50f, PlayerDetect);
            currentTarget = (colliders.Length > 0 && colliders[0] != null) ? colliders[0].transform : null;
        }
    }

    private Transform FindNearestOtherEnemy()
    {
        EnemiesHealth[] enemies = FindObjectsOfType<EnemiesHealth>();
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (EnemiesHealth otherHealth in enemies)
        {
            if (otherHealth == this || otherHealth == null || otherHealth.isCurrentlyInfected) continue;

            float dist = Vector3.Distance(transform.position, otherHealth.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestEnemy = otherHealth.transform;
            }
        }
        return closestEnemy;
    }

    // --- DAMAGE & DEATH ---
    public void TakeDamage(int damage)
    {
        health -= damage;
        damagish = true;
        changeColorTimeTimer = colorChangeDuration;

        if (DamageExplode && ExplodePrefab != null)
        {
            SpawnExplosion();
            DamageExplode = false;
        }

        if (health <= 0)
        {
            health = 0;
            Die();
        }
        else
        {
            if (sound != null && SoundDamage != null)
            {
                sound.PlayOneShot(SoundDamage);
            }
        }

        if (damage > 2 && rb != null)
        {
            Vector3 jumpDirection = -transform.forward + Vector3.up * 0.5f;
            rb.AddForce(jumpDirection.normalized * DamageHitRange, ForceMode.Impulse);
            if (anime != null) anime.SetTrigger("Damage");
        }

        if (animes != null) animes.SetTrigger("DamageRed");
    }

    public void Die()
    {
        if (anime != null) anime.SetTrigger("Die");

        BloodFly bloodFly = GetComponent<BloodFly>();
        if (bloodFly != null) bloodFly.enabled = true;

        // Disable scripts other than self and BloodFly
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != this && !(script is BloodFly))
            {
                script.enabled = false;
            }
        }

        Destroy(gameObject, ExplodeTime);
    }

    public void takedamage()
    {
        if (sound != null && clipsound != null)
        {
            sound.PlayOneShot(clipsound);
        }
    }

    private void SpawnExplosion()
    {
        Transform exp = Instantiate(ExplodePrefab, transform.position, transform.rotation);
        Destroy(exp.gameObject, 4f);
        foreach (Rigidbody expRb in exp.GetComponentsInChildren<Rigidbody>())
        {
            expRb.AddExplosionForce(10, transform.position, 5);
        }
    }

    private void OnDestroy()
    {
        // Grant XP
        WeaponsUI ui = FindObjectOfType<WeaponsUI>();
        if (ui != null)
        {
            ui.AddXP(LevelXp);
        }

        // Destroy active particle clone, NOT prefab asset
        if (infectorClone != null) Destroy(infectorClone);

        if (!DamageExplode && ExplodePrefab != null)
        {
            SpawnExplosion();
        }

        // Mission Enemy Notification
        if (IsMissionEnemy)
        {
            if (MissionSound.MissionSound_ != null)
            {
                MissionSound.MissionSound_.Mission4(IndexMission);
            }

            foreach (EnemiesHealth en in FindObjectsOfType<EnemiesHealth>())
            {
                en.IsMissionEnemy = false;
            }
        }

        if (Bolt != null)
        {
            Instantiate(Bolt, transform.position, transform.rotation);
        }

        // Cleanup references in managers
        if (RocketMission.RocketMission_ != null && RocketMission.RocketMission_.gameObject.activeSelf)
        {
            RocketMission.RocketMission_.DropShip.Remove(gameObject);
            RocketMission.RocketMission_.Enemies.Remove(gameObject);
            RocketMission.RocketMission_.Rockets.Remove(gameObject);
        }

        SpawnTime spawnTime = FindObjectOfType<SpawnTime>();
        if (spawnTime != null)
        {
            if (spawnTime.DropshipsSpawned != null) spawnTime.DropshipsSpawned.Remove(gameObject);
            if (spawnTime.EnemiesSpawned != null) spawnTime.EnemiesSpawned.Remove(gameObject);
        }

        foreach (GalacticRangers gl in FindObjectsOfType<GalacticRangers>())
        {
            gl.IsShooting = false;
        }
    }

    private IEnumerator LaserInitializationWait()
    {
        ThyrranoidLaser laser = GetComponent<ThyrranoidLaser>();
        if (laser != null)
        {
            laser.SePlayer = false;
            yield return new WaitForSeconds(3f);
            laser.SePlayer = true;
        }
    }
}