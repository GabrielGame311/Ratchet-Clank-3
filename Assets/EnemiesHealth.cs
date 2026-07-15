using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesHealth : MonoBehaviour, IInfectable
{
    public float colorChangeDuration = 0.2f;

    public Renderer[] MaterialRed;
    public int health = 1;
    public int maxHealth = 0;
    private GameObject enemie;
    public Animator anime;
    private AudioSource sound;
    public AudioClip clipsound;
    public bool destroy = false;
    public GameObject Bolt;
    public float knockbackForce = 2;
    public Transform ExplodePrefab;
    bool play = false;
    public AudioClip SoundDamage;
    public bool BossHealth = false;
    public float LevelXp = 0.5f;
    public static EnemiesHealth EnemieHealth_;
    public Color damageColor = Color.red;         // Färg för skada (röd)
    public Color startColor = Color.white;        // Ursprunglig färg
    public Animator animes;

    public bool DamageExplode = false;
    public float ChangeColorTime = 0.2f;
    public bool damagish = false;

    public float ExplodeTime;
    public float DamageHitRange;
    public GameObject InfectorEffect;
    Rigidbody rb;

    [Header("Infector & Targeting System")]
    public bool IsInfector = false;
    public LayerMask PlayerDetect;
    
    [Tooltip("Detta är målet som dina andra skript (rörelse/skytte) ska jaga!")]
    public Transform currentTarget; 

    private bool isCurrentlyInfected = false;
    private float targetScanTimer = 0f;
    private float infectionTimer = 0f;
    private float currentInfectionDuration = 15f;

    [Tooltip("Hur länge infektionen varar på Level 1")]
    public float baseInfectionDuration = 15f; 
    [Tooltip("Hur många extra sekunder man får per nivå över Level 1")]
    public float durationIncreasePerLevel = 5f; 
    GameObject InfeCtorClone;
    private void Start()
    {
        MaterialRed = GetComponentsInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();

        if (BossHealth)
        {
            EnemieHealth_ = this;
        }

        StartCoroutine(Wait());
        anime = GetComponentInChildren<Animator>();
        maxHealth = health;
        enemie = gameObject;
        sound = GetComponent<AudioSource>();

        // Kör en första sökning direkt vid start
        FindTarget();
    }

    private void Update()
    {
        // Hantera färgblinkning vid skada
        if (damagish)
        {
            ChangeColorTime -= Time.deltaTime;
            if (ChangeColorTime < 0)
            {
                damagish = false;
                ChangeColorTime = 0.2f;
                foreach (Renderer renderer in MaterialRed)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.color = startColor;
                    }
                }
            }
            else
            {
                foreach (Renderer renderer in MaterialRed)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.color = damageColor;
                    }
                }
            }
        }

        // --- INFECTOR: Nedräkning av tid ---
        if (isCurrentlyInfected)
        {
            infectionTimer += Time.deltaTime;
            if (infectionTimer >= currentInfectionDuration)
            {
                

                Destroy(InfeCtorClone);
                
                Infect(false);
                Debug.Log($"{gameObject.name} är inte längre infekterad.");
            }
        }

        // Sök efter mål med jämna mellanrum (optimerat till var 0.2:e sekund istället för varje frame)
        targetScanTimer += Time.deltaTime;
        if (targetScanTimer > 0.2f)
        {
            targetScanTimer = 0f;
            FindTarget();
        }

        // --- GEMENSAMT KROCK-SKYDD FÖR ALLA FIENDER ---
        // Om vi inte är infekterade och har ett giltigt mål som inte är en annan fiende (dvs vi jagar spelaren)
        if (!isCurrentlyInfected && currentTarget != null && !currentTarget.CompareTag("Enemie"))
        {
            float distanceFromEnemy = 5f; 
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy == this.gameObject || enemy == null) continue;

                float distances = Vector3.Distance(transform.position, enemy.transform.position);
                if (distances < distanceFromEnemy)
                {
                    Vector3 direction = (enemy.transform.position - transform.position).normalized;
                    Vector3 newPosition = transform.position + direction * distanceFromEnemy;
                    enemy.transform.position = newPosition;
                }
            }
        }
    }

    // --- MÅLSÖKNINGSLOGIK (Gemensam för ALLA fiender!) ---
    private void FindTarget()
    {
        if (isCurrentlyInfected)
        {
            // Sök efter närmaste FIENDE att attackera
            currentTarget = FindNearestOtherEnemy();
        }
        else
        {
            // Sök efter SPELAREN att attackera
            Collider[] colliders = Physics.OverlapSphere(transform.position, 50f, PlayerDetect);
            if (colliders.Length > 0 && colliders[0] != null)
            {
                currentTarget = colliders[0].transform;
            }
            else
            {
                currentTarget = null;
            }
        }
    }

    private Transform FindNearestOtherEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == this.gameObject || enemy == null) continue;

            // Undvik att attackera kompisar som också är infekterade
            EnemiesHealth otherHealth = enemy.GetComponent<EnemiesHealth>();
            if (otherHealth != null && otherHealth.isCurrentlyInfected) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestEnemy = enemy.transform;
            }
        }
        return closestEnemy;
    }

    // --- IInfectable Implementering ---
    public void Infect(bool state)
    {
        Infect(state, 1);
    }

    public void Infect(bool state, int level)
    {
        isCurrentlyInfected = state;
        IsInfector = state; 
        // 1. Spawna effekten på fiendens position
       // 1. Spawna din partikeleffekt
        GameObject effect = Instantiate(InfectorEffect, transform.position, transform.rotation);
        InfeCtorClone = effect;

        // 2. Fäst den på fienden
        InfeCtorClone.transform.SetParent(this.transform);
        InfeCtorClone.transform.localPosition = Vector3.zero; // Nollställ så den sitter mitt på

        // 3. Hämta fiendens SkinnedMeshRenderer (den animerade kroppen)
        SkinnedMeshRenderer enemyMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        ParticleSystem ps = InfeCtorClone.GetComponent<ParticleSystem>();

        if (enemyMesh != null && ps != null)
        {
            // Hämta Shape-modulen i partikelsystemet
            var shape = ps.shape;
            
            // Byt shape till Skinned Mesh Renderer och tilldela fiendens mesh!
            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            shape.skinnedMeshRenderer = enemyMesh;
        }
        if (state)
        {
            int clampedLevel = Mathf.Clamp(level, 1, 5);
            currentInfectionDuration = baseInfectionDuration + ((clampedLevel - 1) * durationIncreasePerLevel);
            infectionTimer = 0f;
            Debug.Log($"{gameObject.name} infekterad på Lvl {clampedLevel} i {currentInfectionDuration} sekunder!");
        }
        else
        {
            Destroy(InfeCtorClone);
            currentTarget = null;
        }
        
        FindTarget(); 
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        damagish = true;

        if (DamageExplode)
        {
            if (ExplodePrefab != null)
            {
                Transform exp = Instantiate(ExplodePrefab, transform.position, transform.rotation);
                Destroy(exp.gameObject, 4);
                foreach (Rigidbody gm in exp.GetComponentsInChildren<Rigidbody>())
                {
                    gm.AddExplosionForce(10, transform.position, 5);
                    DamageExplode = false;
                    ExplodePrefab = null;
                }
            }
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
        if (anime != null)
        {
            anime.SetTrigger("Die");
        }

        if (GetComponent<BloodFly>() != null)
        {
            GetComponent<BloodFly>().enabled = true;
        }

        Destroy(gameObject, ExplodeTime);

        // Stäng av alla andra beteendeskript dynamiskt vid död
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != this && script.GetType() != typeof(BloodFly))
            {
                script.enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        WeaponsUI ui = FindObjectOfType<WeaponsUI>();
        if (ui != null)
        {
            ui.levelAmount += LevelXp;
        }
        if(InfectorEffect != null)
        {
            Destroy(InfectorEffect);
        }
        if (!DamageExplode)
        {
            if (ExplodePrefab != null)
            {
                Transform exp = Instantiate(ExplodePrefab, transform.position, transform.rotation);
                Destroy(exp.gameObject, 4);
                foreach (Rigidbody gm in exp.GetComponentsInChildren<Rigidbody>())
                {
                    gm.AddExplosionForce(10, transform.position, 5);
                }
            }
        }

        if (Bolt != null)
        {
            Instantiate(Bolt, transform.position, transform.rotation);
        }

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

    public void takedamage()
    {
        if (sound != null && clipsound != null)
        {
            sound.PlayOneShot(clipsound);
        }
    }

    IEnumerator Wait()
    {
        ThyrranoidLaser laser = GetComponent<ThyrranoidLaser>();
        if (laser != null)
        {
            laser.SePlayer = false;
            yield return new WaitForSeconds(3);
            laser.SePlayer = true;
        }
    }
}