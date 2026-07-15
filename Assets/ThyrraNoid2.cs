using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThyrraNoid2 : MonoBehaviour
{
    [Header("Movement & Separation")]
    public float MoveSpeed;
    public float Distance;
    public float minDistanceFromFirstEnemy;

    [Header("Combat & Shooting")]
    public float ShootTime;
    private float startShoot;
    public bool SePlayer = false;
    public bool Shooting = false;
    public GameObject Gun;
    public Transform pointGun;
    public float ShootForce;
    public GameObject ParticlePrefab;

    [Header("Distance Settings")]
    public float DistanceFromPlayer;

    [Header("Audio")]
    private AudioSource sound;
    public AudioClip[] SoundFX;
    public int SoundplayInt = 0;

    [Header("Components")]
    public Animator anime;
    private EnemiesHealth myHealth;
    private Transform currentTarget; // Hämtas dynamiskt från EnemiesHealth!

    void Start()
    {
        sound = GetComponent<AudioSource>();
        anime = GetComponentInChildren<Animator>();
        myHealth = GetComponent<EnemiesHealth>();
        startShoot = ShootTime;
    }

    void Update()
    {
        // 1. SEPARATIONSLOGIK (Håll avstånd till andra fiender)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");

        foreach (GameObject enemy in enemies)
        {
            if (enemy == this.gameObject) continue; // Undvik att trycka bort oss själva

            float distances = Vector3.Distance(transform.position, enemy.transform.position);

            if (distances < minDistanceFromFirstEnemy)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                Vector3 newPosition = transform.position + direction * minDistanceFromFirstEnemy;
                enemy.transform.position = newPosition;
            }
        }

        // 2. HÄMTA AKTUELLT MÅL FRÅN ENEMIESHEALTH
        currentTarget = (myHealth != null) ? myHealth.currentTarget : null;

        if (currentTarget == null)
        {
            anime.SetBool("Run", false);
            SePlayer = false;
            return;
        }

        // 3. LOGIK FÖR ATT UPPTÄCKA MÅLET
        float dis = Vector3.Distance(transform.position, currentTarget.position);

        if (dis < Distance)
        {
            SePlayer = true;
        }
        else
        {
            SePlayer = false;
        }

        // 4. SKJUT- OCH JAKTLOGIK
        if (Shooting)
        {
            if (ShootTime < 3)
            {
                Shooting = false;
                anime.SetTrigger("Shoot");
            }
        }
        else
        {
            if (SePlayer)
            {
                float disp = Vector3.Distance(transform.position, currentTarget.position);

                if (DistanceFromPlayer < disp)
                {
                    // Spring mot målet
                    transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, MoveSpeed * Time.deltaTime);
                    transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));
                    anime.SetBool("Run", true);
                }
                else
                {
                    // Stå still och titta på målet
                    anime.SetBool("Run", false);
                    transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));
                    SePlayer = false;
                }

                ShootTime -= Time.deltaTime;

                if (ShootTime < 0)
                {
                    ShootTime = startShoot;
                    Shooting = true;
                }
            }
            else
            {
                anime.SetBool("Run", false);
            }
        }
    }

    public void Shoot()
    {
        if (SoundFX.Length > 0)
        {
            SoundplayInt = 0;
            sound.PlayOneShot(SoundFX[SoundplayInt]);
        }
    }

    public void ShootParticle()
    {
        if (ParticlePrefab == null || pointGun == null) return;

        // Skjut partikeln rakt framåt i den riktning som pistolen/fienden pekar mot målet
        GameObject prefab = Instantiate(ParticlePrefab, pointGun.position, pointGun.rotation);

        Rigidbody bulletRb = prefab.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = pointGun.forward * ShootForce;
        }

        ParticleSystem ps = ParticlePrefab.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
        }

        Destroy(prefab, 15f);
    }

    public void Glad()
    {
        if (SoundFX.Length > 1)
        {
            SoundplayInt = 1;
            sound.PlayOneShot(SoundFX[SoundplayInt]);
        }
    }
}