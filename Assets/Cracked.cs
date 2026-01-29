using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cracked : MonoBehaviour
{
   
    public float explosionForce = 1000f; // The force applied to objects in the explosion.
    public float explosionRadius = 5f; // The radius of the explosion.

   

    private void Start()
    {
        Explode();
    }

    void Explode()
    {
       

       

        // Get all colliders within the explosion radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Apply explosion force to nearby objects with rigidbodies.
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

       
    }
}
