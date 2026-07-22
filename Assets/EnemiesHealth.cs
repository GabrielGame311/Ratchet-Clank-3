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

    [Header("Kantskydd (Utan NavMesh)")]
    [Tooltip("Hur långt framför fienden vi ska söka efter mark (bör matcha fiendens radie + marginal)")]
    public float edgeCheckDistance = 0.6f;
    [Tooltip("Vilket Layer som räknas som mark/broar så att vi inte kliver på tomma intet")]
    public LayerMask groundLayer;

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

    private void FixedUpdate()
    {
        // --- 1. PROAKTIVT KANTSKYDD (STOPPAR RÖRELSEN INNAN DE KLIVER AV) ---
        if (rb != null)
        {
            // Ta reda på vilken riktning fienden faktiskt försöker röra sig i
            Vector3 moveDirection = rb.linearVelocity;
            moveDirection.y = 0; // Vi bryr oss bara om rörelse på X- och Z-axeln

            // Om de rör på sig, gör en koll framåt
            if (moveDirection.magnitude > 0.05f)
            {
                Vector3 normalizedDir = moveDirection.normalized;
                
                // Positionen framför fienden där de är på väg att sätta sin fot
                Vector3 checkPosition = transform.position + (normalizedDir * edgeCheckDistance);
                
                // Vi startar Raycasten en bit ovanför fötterna och skjuter neråt
                Vector3 rayOrigin = checkPosition + Vector3.up * 1.0f; 
                
                // Skjut en stråle rakt ner för att se om det finns mark framför oss
                if (!Physics.Raycast(rayOrigin, Vector3.down, 1.5f, groundLayer))
                {
                    // OJ! Det finns ingen mark framför oss! 
                    // Vi stoppar omedelbart all fart i rörelseriktningen så att de "krockar" med kanten
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    
                    // Knuffa tillbaka dem ytterst lite så att de inte "hänger" över kanten
                    rb.position -= normalizedDir * 0.05f;
                }
            }
        }
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

        // Sök efter mål med jämna mellanrum
        targetScanTimer += Time.deltaTime;
        if (targetScanTimer > 0.2f)
        {
            targetScanTimer = 0f;
            FindTarget();
        }

        // --- GEMENSAMT KROCK-SKYDD FÖR ALLA FIENDER ---
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

    // --- MÅLSÖKNINGSLOGIK ---
    private void FindTarget()
    {
        if (isCurrentlyInfected)
        {
            currentTarget = FindNearestOtherEnemy();
        }
        else
        {
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

        if(InfeCtorClone == null)
        {
            GameObject effect = Instantiate(InfectorEffect, transform.position, transform.rotation);
            InfeCtorClone = effect;
        }
        
       

        InfeCtorClone.transform.SetParent(this.transform);
        InfeCtorClone.transform.localPosition = Vector3.zero;

        SkinnedMeshRenderer enemyMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        ParticleSystem ps = InfeCtorClone.GetComponent<ParticleSystem>();

        if (enemyMesh != null && ps != null)
        { 
            var shape = ps.shape;
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