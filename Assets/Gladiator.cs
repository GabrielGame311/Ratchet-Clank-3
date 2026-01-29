using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gladiator : MonoBehaviour
{

    public Animator anime;
    public int Damage;
    public float MoveSpeed;
    public float SePlayerDistance;
    public float DistanceAttack;
    GameObject Player_;
    public float RotateSpeed;
    public float TimeAttack;
    float StartAttack;

    public float SeparationRadius = 1.5f; // Radius to maintain distance from other enemies
    public float SeparationForce = 2f;   // Strength of the repelling force


    // Start is called before the first frame update
    void Start()
    {
        Player_ = GameObject.FindGameObjectWithTag("Player");

        StartAttack = TimeAttack;
    }

    // Update is called once per frame
    void Update()
    {

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        if (transform.position.y < 0)
        {
            GetComponent<EnemiesHealth>().TakeDamage(100);
            Destroy(gameObject);
        }
        float dis = Vector3.Distance(transform.position, Player_.transform.position);


        if(dis < SePlayerDistance)
        {
            if(dis > DistanceAttack)
            {
                transform.position = Vector3.MoveTowards(transform.position, Player_.transform.position, MoveSpeed * Time.deltaTime);

               
                Vector3 direction = (Player_.transform.position - transform.position).normalized;

                // Beräkna målrotationen med riktningen mot spelaren
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                // Roterar objektet långsamt mot spelaren med RotateTowards
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);

                anime.SetBool("Run", true);
               
                
                  
                      

                       


                    
                   
                

               
            }
            else
            {
                TimeAttack -= Time.deltaTime;

                if (TimeAttack < 0)
                {

                    Attack();
                    TimeAttack = StartAttack;
                }
            }


        }
        else
        {
            anime.SetBool("Run", false);
        }

        ApplySeparation();


       
    }

    
  

    private void ApplySeparation()
    {
        // Get all colliders within the separation radius
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, SeparationRadius);

        foreach (Collider other in nearbyEnemies)
        {
            // Check if the other object is an enemy and is not this object
            if (other.CompareTag("Enemie") && other.gameObject != this.gameObject)
            {
                // Calculate the repelling direction
                Vector3 repellingDirection = (transform.position - other.transform.position).normalized;

                // Apply the repelling force
                transform.position += repellingDirection * SeparationForce * Time.deltaTime;
            }
        }

        Collider[] nearbyHoles = Physics.OverlapSphere(transform.position, SeparationRadius);
        foreach (Collider other in nearbyHoles)
        {
            // Check if the collider is a hole
            if (other.CompareTag("Hole"))
            {
                // Calculate a tangential direction to go around the hole
                Vector3 directionToHole = (other.transform.position - transform.position).normalized;
                Vector3 tangentialDirection = Vector3.Cross(directionToHole, Vector3.up).normalized;

                // Decide which tangential direction to take
                Vector3 escapeDirection = Vector3.Dot(transform.forward, tangentialDirection) > 0 ? tangentialDirection : -tangentialDirection;

                // Move the enemy around the hole
                transform.position += escapeDirection * SeparationForce * Time.deltaTime;
            }
        }

    }

    public void TakeDamage()
    {
        Player_.GetComponent<Player>().TakeDamage(Damage);
    }

    public void Attack()
    {
        anime.SetTrigger("Attack");
    }

}
