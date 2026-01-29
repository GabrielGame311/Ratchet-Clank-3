using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorretRanger : MonoBehaviour
{

    public Transform Hand;
    public GameObject Enemie;
    public GameObject Bullet;
    public Transform Point1;
    public Transform Point2;
    public float DistanceEnemie;
    public float BulletSpeed;
    bool IsEnemie;
    public float RotateSpeed;
    public int IntShoot;
    public float ShootTime;
    float StartTime;
    public Animator anime;
    AudioSource Sound;
    public AudioClip SoundClip;

    // Start is called before the first frame update
    void Start()
    {

        StartTime = ShootTime;

        Sound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        float dis = Vector3.Distance(Hand.transform.position, Enemie.transform.position);

        if(dis < DistanceEnemie)
        {

            RotateTowardsEnemy();
            Shoot();
            
           
            anime.enabled = false;
        }
        else
        {
           
            anime.enabled = true;
        }
      
        if(Enemie == null)
        {
            anime.enabled = true;
        }
        


    }


    void RotateTowardsEnemy()
    {
        // Calculate the direction to the enemy
        Vector3 directionToEnemy = Enemie.transform.position - Hand.transform.position;

        // Calculate the rotation needed to look at the enemy
        Quaternion targetRotation = Quaternion.LookRotation(directionToEnemy);

        // Smoothly rotate towards the target rotation using RotateSpeed
        Hand.transform.rotation = Quaternion.RotateTowards(Hand.transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        ShootTime -= Time.deltaTime;
        
        if(ShootTime < 0)
        {
            Sound.PlayOneShot(SoundClip);
            ShootTime = StartTime;

            if (IntShoot == 0)
            {
                GameObject prefab1 = Instantiate(Bullet, Point1.transform.position, Point1.transform.rotation);
                prefab1.GetComponent<Rigidbody>().velocity = Point1.transform.forward * BulletSpeed;
                IntShoot = 1;
            }
            else if (IntShoot == 1)
            {

                GameObject prefab1 = Instantiate(Bullet, Point2.transform.position, Point2.transform.rotation);
                prefab1.GetComponent<Rigidbody>().velocity = Point2.transform.forward * BulletSpeed;
                IntShoot = 0;
            }
        }


       
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "DropShip")
        {
            //isMoving = false;

            Enemie = other.gameObject;
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "DropShip")
        {
            //isMoving = false;

            Enemie = other.gameObject;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "DropShip")
        {
            //ContinueMove = true;
            //isMoving = false;
            Enemie = null;
            anime.enabled = true;

        }
    }
}
