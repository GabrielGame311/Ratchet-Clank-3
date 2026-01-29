using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;



public class EnemiesDamage : MonoBehaviour
{

    public int laserDamage = 10; // Damage the laser deals
    public float laserRange = 20f;  // Range of the laser
    public LayerMask hitLayers;     // Layers to detect (e.g., Player layer)
    public float laserDuration = 0.1f; // Duration of the laser visual effect

    private LineRenderer laserLine;   // The LineRenderer component to represent the laser
    private float laserTimer;         // Timer to handle visual effect duration
    private bool laserHitPlayer = false; // Flag to ensure laser only damages once per hit

    void Start()
    {
        laserLine = GetComponent<LineRenderer>(); // Get the LineRenderer (for laser visualization)
        laserTimer = laserDuration;
    }

    void Update()
    {
        ShootLaser();
    }

    void ShootLaser()
    {
        RaycastHit hit;
        laserHitPlayer = false; // Reset the flag for this frame

        // Show the laser visual effect (LineRenderer) only if there's a hit
        if (Physics.Raycast(transform.position, transform.forward, out hit, laserRange, hitLayers))
        {
            laserLine.SetPosition(0, transform.position); // Start position of the laser
            laserLine.SetPosition(1, hit.point); // End of the laser (where it hits)

            // Apply damage only if the raycast hits the player and hasn't hit it already
            if (hit.collider.CompareTag("Player") && !laserHitPlayer)
            {
                hit.collider.GetComponent<Player>().TakeDamage(laserDamage);
                Destroy(GetComponent<EnemiesDamage>());
                laserHitPlayer = true; // Set the flag to prevent further damage in the same frame
            }

            // Trigger any additional visual effects at the hit point if needed (e.g., spark or explosion)
            // Example: Instantiate(laserHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
        else
        {
            // If no hit, show the laser reaching max range
            laserLine.SetPosition(0, transform.position);
            laserLine.SetPosition(1, transform.position + transform.forward * laserRange);
        }

        // Decrease the laser effect timer
        laserTimer -= Time.deltaTime;
        if (laserTimer <= 0)
        {
            laserLine.enabled = false; // Disable LineRenderer after laser duration
        }
    }
}
