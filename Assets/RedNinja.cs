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
    



    public bool SePlayer = false;


    //Patrol
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
        
        StartTime = DamageTime;
        if(DiscNinja_)
        {
            anime.SetBool("idleDisc", true);
        }
       
    }

    // Update is called once per frame
    void Update()
    {


        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");




        foreach (GameObject enemy in enemies)
        {


            float distances = Vector3.Distance(transform.position, enemy.transform.position);

            if (distances < minDistanceFromFirstEnemy)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                Vector3 newPosition = transform.position + direction * minDistanceFromFirstEnemy;
                enemy.transform.position = newPosition;
            }
        }


        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        float mindis = Vector3.Distance(transform.position, Player.transform.position);


        if(FromPlayerDistance < mindis)
        {
            //Ispatroling = true;
            SePlayer = false;
            if (Ispatroling)
            {
                anime.SetBool("Walk", true);



                if (0 <= PatrolTime)
                {
                    PatrolTime -= Time.deltaTime;

                    RaycastHit hit;
                    // Vi skjuter en stråle från ninjans position framåt
                    if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, detectionDistance))
                    {
                        // Om vi ser något med en "Wall" tag eller bara vad som helst som inte är spelaren
                        if (!hit.collider.CompareTag("Player"))
                        {
                            StopAndTurn();
                            return;
                        }
                    }

                    // 2. Gå framåt
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
        else
        {
            
            SePlayer = true;
            anime.SetBool("Walk", false);
            Ispatroling = false;
        }

        float DistanceAttack = Vector3.Distance(transform.position, Player.transform.position);

        if (DistanceAttack < AttackDistance)
        {
            if(DiscNinja_ == false)
            {
                anime.SetBool("Run", false);
                DamageTime -= Time.deltaTime;

                if (DamageTime < 0)
                {

                    TakeDamages();

                    DamageTime = StartTime;

                }
            }

        }
        else
        {
            if (SePlayer)
            {
                Ispatroling = false;
                if(DiscNinja_ == false)
                {
                    
                    if(Isrunning)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, Player.transform.position, MoveSpeed * Time.deltaTime);
                    }

                    anime.SetBool("Run", true);
                }
                else
                {
                    DamageTime -= Time.deltaTime;
                    Vector3 direction = Player.transform.position - ShootPoint.position;
                    direction.y = 0; // Ignore the y-axis to prevent vertical rotation

                    if (direction != Vector3.zero) // Ensure the direction is not zero
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
               
                transform.LookAt(Player.transform);

            }
            else
            {
                Isrunning = false;
                anime.SetBool("Run", false);
                

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
        Idletime = 1f; // Kort paus vid krock

        // Vänd dig bort från hindret direkt
        transform.Rotate(0, 180, 0);
    }

    void TakeDamages()
    {

        anime.SetTrigger("Attack");
        Player.GetComponent<Player>().TakeDamage(TakeDamage);
        




    }
    public void Shoot()
    {
       
        anime.SetTrigger("Attack");

      



    }

    public void Shooting()
    {

        GameObject prefabs = Instantiate(prefabshoot, ShootPoint.transform.position, ShootPoint.transform.rotation);
        prefabs.GetComponent<Rigidbody>().linearVelocity = ShootPoint.transform.forward * ShootSpeed;

        Destroy(prefabs, 5);
    }
}
