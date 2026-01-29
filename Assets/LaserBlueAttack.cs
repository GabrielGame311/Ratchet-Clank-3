using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class LaserBlueAttack : MonoBehaviour
{
    public float MoveSpeed;
    public float RotateSpeed;
    public float PlayerDistance;
    public float SePlayerDistance;
    GameObject Player_;
    public Animator anime;
    public ParticleSystem Particle;

    public float CountDownShoot;
    float StartCountDown;

    public float SeparationRadius = 1.5f; // Radius to maintain distance from other enemies
    public float SeparationForce = 2f;   // Strength of the repelling force


    // Start is called before the first frame update
    void Start()
    {
        Player_ = GameObject.FindGameObjectWithTag("Player");
        StartCountDown = CountDownShoot;
    }

    // Update is called once per frame
    void Update()
    {

        float dis = Vector3.Distance(transform.position, Player_.transform.position);


        if(dis < SePlayerDistance)
        {

            if(dis > PlayerDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, Player_.transform.position, Time.deltaTime * MoveSpeed);

                Vector3 direction = Player_.transform.position - transform.position;
                direction.y = 0; // Keep rotation only on the Y axis (optional)

                // If there's a valid direction
                if (direction != Vector3.zero)
                {
                    // Calculate target rotation
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    // Smoothly interpolate towards the target rotation
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
                }


            }
            else
            {
                CountDownShoot -= Time.deltaTime;

                if(CountDownShoot < 0)
                {
                    Shoot();
                    CountDownShoot = StartCountDown;
                }
            }
        }
        else
        {


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

    public void Shoot()
    {
        anime.SetTrigger("Shoot");
        Particle.Play();
    }
}
