using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thyrranoidship : MonoBehaviour
{

    public Transform Point1;
    public Transform Point2;

    public GameObject BulletPrefab;

    public float ShootTime;
    float StartShoot;
  
  
    private bool isShooting = false;
    public bool SePlayer;

    GameObject Player;
    private float shootingTime = 4.0f;
    public float waitTime = 3.0f;
    private float timer = 0.0f;
    Animator anime;
    public bool Shoot = false;
    public float MoveSpeed;
    public float PlayerDis;
    public float ShootForce;
    public bool IsFlying = false;

    private float currentRotation = 0f;

    public float rotationSpeed = 50.0f;
    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    int soundPlaying;
    public float Rotation;
    // Start is called before the first frame update
    void Start()
    {
        currentWaypointIndex = 0;

        StartShoot = ShootTime;



        anime = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
       
        if (SePlayer)
        {

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Player.transform.rotation, 3 * Time.deltaTime);

            transform.LookAt(Player.transform);

            float dis = Vector3.Distance(transform.position, Player.transform.position);

            if (PlayerDis < dis)
            {
                IsFlying = false;
                transform.position = Vector3.MoveTowards(transform.position, Player.transform.position, MoveSpeed * Time.deltaTime);
              
               
            }
            else
            {
                SePlayer = false;
                
            }

            

                if (!isShooting && timer >= waitTime)
                {
                    isShooting = true;
                    timer = 0.0f;
                }

                // If shooting time is over, stop shooting and start waiting
                if (isShooting && timer >= shootingTime)
                {
                    isShooting = false;
                    timer = 0.0f;
                }

                // Update timer
                timer += Time.deltaTime;

                // Shoot if in shooting state
                if (isShooting)
                {
                    anime.SetTrigger("Shoot");
                }

            
           

        }
        else
        {

            if(Player == null)
            {
               // transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);

            }


            // transform.position += Vector3.forward * MoveSpeed * Time.deltaTime;

            

        
        }

     




        if (IsFlying)
        {


            if (currentWaypointIndex < waypoints.Length)
            {
                Vector3 targetPosition = waypoints[currentWaypointIndex].position;

                // Calculate the direction to the current waypoint.
                Vector3 direction = targetPosition - transform.position;

                // Calculate the distance to the current waypoint.
                float distanceToWaypoint = Vector3.Distance(transform.position, targetPosition);

                
                    // Rotate towards the waypoint.
                    Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotationSpeed * Time.deltaTime);

                    // Move towards the waypoint.
                    transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
                

                // Check if the ship is close enough to the current waypoint.
                if (distanceToWaypoint < 1.0f)
                {
                    // Move to the next waypoint.
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypoints.Length)
                    {
                        // If we reached the end of the waypoints, start over.
                        currentWaypointIndex = 0;
                    }
                }
            }




        }









    }

    public void Shooting()
    {

        GameObject prefab = Instantiate(BulletPrefab, Point1.transform.position, Point1.transform.rotation);
        GameObject prefab2 = Instantiate(BulletPrefab, Point2.transform.position, Point2.transform.rotation);


        prefab.GetComponent<Rigidbody>().linearVelocity = Point1.transform.forward * ShootForce;
        prefab2.GetComponent<Rigidbody>().linearVelocity = Point2.transform.forward * ShootForce;


        Destroy(prefab, 8);
        Destroy(prefab2, 8);

    }


    private void OnTriggerStay(Collider other)
    {
        
        if(other.tag == "Player")
        {
            Player = other.gameObject;
            SePlayer = true;
        }

        if (other.tag == "RangerFall")
        {
            Player = other.gameObject;
            SePlayer = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            SePlayer = false;
        }
        if (other.tag == "RangerFall")
        {
            SePlayer = false;
        }



    }


}
