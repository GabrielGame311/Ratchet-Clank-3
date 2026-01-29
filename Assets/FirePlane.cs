using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePlane : MonoBehaviour
{

    // Define the variables
    public float FireTime = 1f; // Time between activating each fire particle
    private float currentFireTime;
    public GameObject[] FireParticles; // Array of fire particles
    private int currentFireIndex = 0; // Index of the current fire particle
    public float startTime = 1f; // Reset time interval
    public bool isFire = false;
    public FireDamage firedamage;

    void Start()
    {
        currentFireTime = FireTime; // Initialize the timer
       
    }

    void Update()
    {



       
        
        if(isFire == false)
        {
            // Decrease the fire timer
            FireTime -= Time.deltaTime;

            // Check if the timer has elapsed


            // Play the particle at the current index


            if (FireTime < 0)
            {
                FireTime = currentFireTime;

                FirePlay();
            }

        }




    }


    public void FirePlay()
    {
        foreach (GameObject fire in FireParticles)
        {
            fire.GetComponentInChildren<ParticleSystem>().Play();
            firedamage.Collider.enabled = true;
        }
    }

    public void FireStop()
    {
        firedamage.Collider.enabled = false;
        firedamage.IsTrigger = false;
    }

}
