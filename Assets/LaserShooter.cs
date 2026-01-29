using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserShooter : MonoBehaviour
{
    public float rotationSpeed;
    public Transform ShootPoint;
    public float ShootTime;
    float StartShoot;
    public GameObject ParticleLaser;
    public Animator anime;
    GameObject Player_;
    public Transform Head;
    public float SePlayerDis;
    private Quaternion initialRotation;
    // Start is called before the first frame update
    void Start()
    {
        initialRotation = Head.rotation;
        StartShoot = ShootTime;
        Player_ = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {

        float dis = Vector3.Distance(Head.transform.position, Player_.transform.position);

        if (dis < SePlayerDis)
        {
            
            anime.SetBool("SePlayer", true);

            // Step 1: Calculate the direction from the turret head to the player
            Vector3 direction = Player_.transform.position - Head.position;
            direction.y = 0; // Ignore vertical rotation for horizontal-only rotation
            direction.Normalize(); // Normalize the direction to ensure it's just a direction

            // Step 2: Get the current forward direction (blue axis) of the turret head
            Vector3 right = Head.forward; // Using forward to represent the blue axis (Z-axis)
            right.y = 0; // Ignore the vertical axis for horizontal-only rotation
            right.Normalize(); // Normalize the forward direction

            // Step 3: Calculate the angle between the forward direction and the direction to the player
            float angle = Vector3.SignedAngle(right, direction, Vector3.up);

            // Step 4: Clamp the angle to limit the rotation to -180 to 180 degrees
            float clampedAngle = Mathf.Clamp(angle, -90f, 90f);

            // Step 5: Calculate the target rotation based on the clamped angle


            Quaternion finalRotation = Quaternion.Euler(0, Head.eulerAngles.y + clampedAngle, 0);

            // Step 7: Smoothly rotate towards the target rotation
            Head.rotation = Quaternion.Slerp(Head.rotation, finalRotation, rotationSpeed * Time.deltaTime);
            // Step 6: Apply the clamped angle adjustment to the target rotation


            // Step 8: Check if the head has rotated more than 90 degrees (or -90 degrees)
            float currentAngle = Vector3.Angle(initialRotation * Vector3.forward, Head.forward); // Angle from the initial forward to the current forward

            if (currentAngle > 90f) // Check if the turret head has rotated more than 90 degrees
            {
                anime.SetBool("SePlayer", false); // Set the animation to false if the head is rotated too much


            }
            else
            {
                ShootTime -= Time.deltaTime;
                if (ShootTime < 0)
                {
                    ShootTime = StartShoot;
                    Shoot();
                }
            }
           

           
        }
        else
        {
            anime.SetBool("SePlayer", false); // Set the animation to false when player is out of range
        }

    }



    public void Shoot()
    {
        anime.SetTrigger("Shoot");
        GameObject prefab_ = Instantiate(ParticleLaser, ShootPoint.transform.position, ShootPoint.transform.rotation);
        Destroy(prefab_, 5);

    }

}
