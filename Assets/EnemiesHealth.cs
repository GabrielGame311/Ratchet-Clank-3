using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesHealth : MonoBehaviour
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

    Rigidbody rb;

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
    }

    private void Update()
    {
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
                        mat.color = startColor;  // Återställ till startfärgen
                    }
                }
            }
            else
            {
                foreach (Renderer renderer in MaterialRed)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.color = damageColor;  // Sätter materialets färg till röd
                    }
                }
            }
        }
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

        // Förstör fiende-objektet efter explosionstiden
        Destroy(gameObject, ExplodeTime);

        if (GetComponent<MiniThyrra>() != null)
        {
            GetComponent<MiniThyrra>().enabled = false;
        }
        else if (GetComponent<RedNinja>() != null)
        {
            GetComponent<RedNinja>().enabled = false;
        }
    }

    private void OnDestroy()
    {
        // 1. Ge XP till spelarens vapen
        WeaponsUI ui = FindObjectOfType<WeaponsUI>();
        if (ui != null)
        {
            ui.levelAmount += LevelXp;
        }

        // 2. Skapa explosionseffekt om det inte redan har skett
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

        // 3. Spawna Bolts (Valuta)
        if (Bolt != null)
        {
            Instantiate(Bolt, transform.position, transform.rotation);
        }

        // NOTERA: Kodraderna som manuellt tog bort detta gameObject från EnemiesMission.instance.EnemiesList 
        // har plockats bort härifrån. Detta eftersom EnemiesMission.cs nu automatiskt städar bort null-referenser 
        // i sin egen Update-loop på ett säkrare sätt.

        // 4. Hantera RocketMission om det är aktivt
        if (RocketMission.RocketMission_ != null && RocketMission.RocketMission_.gameObject.activeSelf)
        {
            RocketMission.RocketMission_.DropShip.Remove(gameObject);
            RocketMission.RocketMission_.Enemies.Remove(gameObject);
            RocketMission.RocketMission_.Rockets.Remove(gameObject);
        }

        // 5. Hantera SpawnTime-skriptet om det existerar
        SpawnTime spawnTime = FindObjectOfType<SpawnTime>();
        if (spawnTime != null)
        {
            if (spawnTime.DropshipsSpawned != null) spawnTime.DropshipsSpawned.Remove(gameObject);
            if (spawnTime.EnemiesSpawned != null) spawnTime.EnemiesSpawned.Remove(gameObject);
        }

        // Inaktivera alla andra skripter på objektet vid förstörelse
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != this)
            {
                script.enabled = false;
            }
        }

        // Säg till Rangers att sluta skjuta
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