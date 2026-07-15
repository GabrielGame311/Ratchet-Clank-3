using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ThyrranoidLaser : MonoBehaviour
{
    public bool IsIdle = false;
    public float RotateSpeed;
    public GameObject Particle1;
    public GameObject Particle2;

    public Transform Point1;
    public Transform Point2;
    
    public float MoveSpeed;
    public bool IsShoot = false;

    public float ShootTime;
    public float PlayerDistance;
    public float ShootDistance;
    public bool SePlayer = false;
    
    public Animator anime;
    float startShoot;
    public int ShootCount = 0;
    Quaternion rot1;
    Quaternion rot2;

    // Referens till det gemensamma hälsomanuskriptet på samma objekt
    private EnemiesHealth myHealth;

    void Start()
    {
        anime = GetComponentInChildren<Animator>();
        myHealth = GetComponent<EnemiesHealth>();

        rot1 = Point1.transform.rotation;
        rot2 = Point2.transform.rotation;
        startShoot = ShootTime;
    }

    void Update()
    {
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // Hämta målet direkt från EnemiesHealth!
        Transform target = (myHealth != null) ? myHealth.currentTarget : null;

        if (target != null)
        {
            float dis = Vector3.Distance(transform.position, target.position);
            SePlayer = (dis < ShootDistance);
        }
        else
        {
            SePlayer = false;
        }

        // --- RÖRELSE OCH SKUTANDE ---
        if (IsIdle)
        {
            transform.LookAt(Vector3.forward);
            ShootTime -= Time.deltaTime;

            if (ShootTime < 0)
            {
                ShootTime = startShoot;
                IsShoot = true;
            }
        }

        if (IsShoot)
        {
            if (ShootTime < 3)
            {
                IsShoot = false;
                anime.SetTrigger("Shoot");
            }
        }
        else
        {
            if (SePlayer && target != null)
            {
                float disp = Vector3.Distance(transform.position, target.position);

                if (PlayerDistance < disp)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target.position, MoveSpeed * Time.deltaTime);
                    Vector3 direction = (target.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);

                    anime.SetBool("Run", true);
                }
                else
                {
                    anime.SetBool("Run", false);
                    transform.LookAt(target);
                }

                ShootTime -= Time.deltaTime;

                if (ShootTime < 0)
                {
                    ShootTime = startShoot;
                    IsShoot = true;
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
        Transform target = (myHealth != null) ? myHealth.currentTarget : null;
        if (target == null) return;

        if (IsIdle)
        {
            if (ShootCount == 0)
            {
                Point1.transform.rotation = rot1;
                GameObject vs = Instantiate(Particle1, Point1.transform.position, Point1.transform.rotation);
                Destroy(vs, 2);
                ShootCount = 1;
            }
            else if (ShootCount == 1)
            {
                Point2.transform.rotation = rot2;
                GameObject vs = Instantiate(Particle2, Point2.transform.position, Point2.transform.rotation);
                Destroy(vs, 2);
                ShootCount = 0;
            }
        }
        else
        {
            if (ShootCount == 0)
            {
                Point1.transform.LookAt(target);
                GameObject vs = Instantiate(Particle1, Point1.transform.position, Point1.transform.rotation);
                Destroy(vs, 2);
                ShootCount = 1;
            }
            else if (ShootCount == 1)
            {
                Point2.transform.LookAt(target);
                GameObject vs = Instantiate(Particle2, Point2.transform.position, Point2.transform.rotation);
                Destroy(vs, 2);
                ShootCount = 0;
            }
        }
    }
}