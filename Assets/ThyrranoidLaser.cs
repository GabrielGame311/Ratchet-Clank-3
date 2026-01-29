using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;



public class ThyrranoidLaser : MonoBehaviour
{
    public LayerMask PlayerDetect;
    public LayerMask collisionLayer;
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
    public float DistanceFromEnemy = 5;
    GameObject player;
    public Animator anime;
    float startShoot;
    public int ShootCount = 0;
    bool IsActive = false;
    Quaternion rot1;
    Quaternion rot2;
    // Start is called before the first frame update
    void Start()
    {
        
        anime = GetComponentInChildren<Animator>();

        rot1 = Point1.transform.rotation;
        rot2 = Point2.transform.rotation;
        
        startShoot = ShootTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsActive)
        {
            // Get the player by using a raycast or collision detection (or assign player directly in the Inspector)
            Collider[] colliders = Physics.OverlapSphere(transform.position, 50f, PlayerDetect);  // Adjust range accordingly
            if (colliders.Length > 0)
            {
                player = colliders[0].gameObject;  // Assuming the first collider found is the player

                // Check if the player is in the correct layer using LayerMask
                if (player != null && (PlayerDetect.value & (1 << player.layer)) > 0)
                {
                    float dis = Vector3.Distance(transform.position, player.transform.position);

                    if (dis < ShootDistance)
                    {
                        SePlayer = true;
                        IsIdle = false;
                    }
                    else
                    {
                        SePlayer = false;
                    }
                }
            }
        }


        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");


       

        foreach (GameObject enemy in enemies)
        {


            float distances = Vector3.Distance(transform.position, enemy.transform.position);

            if (distances < DistanceFromEnemy)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                Vector3 newPosition = transform.position + direction * DistanceFromEnemy;
                enemy.transform.position = newPosition;
            }
        }

        

       


        if(IsIdle)
        {
            transform.LookAt(Vector3.forward);
            ShootTime -= Time.deltaTime;



            if (ShootTime < 0)
            {

                ShootTime = startShoot;
                IsShoot = true;
            }
        }
        else
        {



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


            if (SePlayer)
            {

                float disp = Vector3.Distance(transform.position, player.transform.position);


                if (PlayerDistance < disp)
                {
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, MoveSpeed * Time.deltaTime);
                    Vector3 direction = (player.transform.position - transform.position).normalized;

                    // Beräkna målrotationen med riktningen mot spelaren
                    Quaternion lookRotation = Quaternion.LookRotation(direction);

                    // Roterar objektet långsamt mot spelaren med RotateTowards
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);

                    anime.SetBool("Run", true);



                }
                else
                {
                    anime.SetBool("Run", false);
                    transform.LookAt(player.transform);

                    SePlayer = false;
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



            }
        }



        



    }


    private void OnTriggerEnter(Collider other)
    {
        if ((PlayerDetect.value & (1 << other.gameObject.layer)) > 0)
        {

        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Kollar om objektet som kolliderar är på ett lager som matchar LayerMask
        if ((collisionLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            IsActive = true;
            Debug.Log("Kollision med rätt lager!");
        }
    }

    public void Shoot()
    {

       


        if(IsIdle)
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
                Point1.transform.LookAt(player.transform);
                GameObject vs = Instantiate(Particle1, Point1.transform.position, Point1.transform.rotation);
                Destroy(vs, 2);

                ShootCount = 1;
            }
            else if (ShootCount == 1)
            {
                Point2.transform.LookAt(player.transform);
                GameObject vs = Instantiate(Particle2, Point2.transform.position, Point2.transform.rotation);
                Destroy(vs, 2);
                ShootCount = 0;
            }
        }

       

    }

}
