using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedNinja : MonoBehaviour
{
    public bool Isrunning = false;
    public bool DiscNinja_;
    public float MoveSpeed;

    public float DamageTime;
    float StartTime;
    public int TakeDamage;
    public float ShootSpeed;
    public Transform ShootPoint;
    public GameObject prefabshoot;
    Animator anime;

    public float AttackDistance;
    public float FromPlayerDistance;

    GameObject Player;
    EnemiesHealth myHealth; // Referens till fiendens hälsoskript

    public bool SePlayer = false;

    // Patrol
    public float PatrolTime;
    public float Idletime;
    float startIdleTime;
    public float WalkSpeed;
    float startPatrol;
    public float detectionDistance = 2f;
    public bool Ispatroling = true;

    //--------------
    float minDistanceFromFirstEnemy = 4;

    // Start is called before the first frame update
    void Start()
    {
        startPatrol = PatrolTime;
        startIdleTime = Idletime;
        Player = GameObject.FindGameObjectWithTag("Player");
        anime = GetComponentInChildren<Animator>();
        
        // Hämta vårt EnemiesHealth-skript för att kunna läsa 'currentTarget'
        myHealth = GetComponent<EnemiesHealth>();
        
        StartTime = DamageTime;
        if(DiscNinja_)
        {
            anime.SetBool("idleDisc", true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Hantera repulsion (undvik att gå in i andra fiender om vi INTE är infekterade och jagar dem)
        bool isCurrentlyInfected = myHealth != null && myHealth.IsInfector;
        if (!isCurrentlyInfected)
        {
            HandleEnemyRepulsion();
        }

        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        // Hitta vem vi ska sikta på/jaga just nu
        Transform target = GetCurrentTarget();

        if (target == null)
        {
            // Om inget mål finns, patrullera eller stå stilla
            PatrolLogic();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Bestäm om vi "ser" eller har upptäckt målet
        if (FromPlayerDistance < distanceToTarget)
        {
            SePlayer = false;
            PatrolLogic();
        }
        else
        {
            SePlayer = true;
            anime.SetBool("Walk", false);
            Ispatroling = false;
        }

        // --- ATTACK & RÖRELSE MOT MÅLET ---
        if (distanceToTarget < AttackDistance)
        {
            if (DiscNinja_ == false)
            {
                anime.SetBool("Run", false);
                DamageTime -= Time.deltaTime;

                if (DamageTime < 0)
                {
                    // Attackera det aktiva målet (Spelare eller annan fiende!)
                    AttackTarget(target);
                    DamageTime = StartTime;
                }
            }
        }
        else
        {
            if (SePlayer)
            {
                Ispatroling = false;
                if (DiscNinja_ == false)
                {
                    if (Isrunning)
                    {
                        // Gå/spring mot vårt aktiva mål
                        transform.position = Vector3.MoveTowards(transform.position, target.position, MoveSpeed * Time.deltaTime);
                    }
                    anime.SetBool("Run", true);
                }
                else
                {
                    DamageTime -= Time.deltaTime;
                    Vector3 direction = target.position - ShootPoint.position;
                    direction.y = 0; // Ignorera höjdskillnader för projektilen

                    if (direction != Vector3.zero) 
                    {
                        Quaternion rotation = Quaternion.LookRotation(direction);
                        ShootPoint.rotation = rotation;
                    }
                    if (DamageTime < 0)
                    {
                        Shoot();
                        DamageTime = StartTime;
                    }
                }
               
                // Vänd dig mot målet (men håll rotationen platt på marken)
                RotateTowards(target.position);
            }
            else
            {
                Isrunning = false;
                anime.SetBool("Run", false);
            }
        }
    }

    // Returnerar vem ninjan ska jaga (Spelaren eller en annan fiende om infekterad)
    private Transform GetCurrentTarget()
    {
        if (myHealth != null && myHealth.currentTarget != null)
        {
            return myHealth.currentTarget;
        }
        
        // Fallback till spelaren om inget annat hittas
        return Player != null ? Player.transform : null;
    }

    private void PatrolLogic()
    {
        if (Ispatroling)
        {
            anime.SetBool("Walk", true);

            if (0 <= PatrolTime)
            {
                PatrolTime -= Time.deltaTime;

                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, detectionDistance))
                {
                    // Om vi krockar med något som inte är vårt nuvarande sök-mål
                    if (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Enemie"))
                    {
                        StopAndTurn();
                        return;
                    }
                }

                transform.Translate(Vector3.forward * WalkSpeed * Time.deltaTime);
            }

            if (PatrolTime <= 0)
            {
                Ispatroling = false;
            }
        }
        else
        {
            anime.SetBool("Walk", false);

            if (0 <= Idletime)
            {
                Idletime -= Time.deltaTime;
            }

            if (Idletime <= 0)
            {
                float randomRotation = Random.Range(90, 270);
                transform.Rotate(0, randomRotation, 0);
                Ispatroling = true;
                PatrolTime = startPatrol;
                Idletime = startIdleTime;
            }
        }
    }

    private void RotateTowards(Vector3 targetPos)
    {
        Vector3 flatTarget = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        transform.LookAt(flatTarget);
    }

    private void AttackTarget(Transform target)
    {
        anime.SetTrigger("Attack");

        if (target.CompareTag("Player"))
        {
            Player playerScript = target.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(TakeDamage);
            }
        }
        else if (target.CompareTag("Enemie"))
        {
            // Infekterad attack mot en annan fiende!
            EnemiesHealth otherEnemy = target.GetComponent<EnemiesHealth>();
            if (otherEnemy != null)
            {
                otherEnemy.TakeDamage(TakeDamage);
            }
        }
    }

    private void HandleEnemyRepulsion()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || enemy == gameObject) continue;

            float distances = Vector3.Distance(transform.position, enemy.transform.position);

            if (distances < minDistanceFromFirstEnemy)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                Vector3 newPosition = transform.position + direction * minDistanceFromFirstEnemy;
                enemy.transform.position = newPosition;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * detectionDistance);
    }

    void StopAndTurn()
    {
        Ispatroling = false;
        Idletime = 1f; 
        transform.Rotate(0, 180, 0);
    }

    public void Shoot()
    {
        anime.SetTrigger("Attack");
    }

    public void Shooting()
    {
        if (prefabshoot == null || ShootPoint == null) return;

        GameObject prefabs = Instantiate(prefabshoot, ShootPoint.transform.position, ShootPoint.transform.rotation);
        
        // Uppdaterad till moderna Unity-fysiknamn (linearVelocity) om du använder det
        Rigidbody rb = prefabs.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = ShootPoint.transform.forward * ShootSpeed;
        }

        Destroy(prefabs, 5);
    }
}