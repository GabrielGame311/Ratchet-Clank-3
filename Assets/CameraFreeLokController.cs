using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFreeLokController : MonoBehaviour
{
    public Transform player;            // Spelarens transform
    public LayerMask groundLayer;       // Lager för marken
    public float ySmoothSpeed = 5f;     // Hastighet för Y-övergång på marken

    private bool isGrounded = true;
    private float targetY;              // Mål-Y-position
    private Vector3 targetPosition;     // Fullständig målposition
    private Rigidbody playerRb;
   
    private bool wasFalling = false;
    public RatchetController playerController;



    private float lastPlayerY; // Håller koll på spelarens Y-position från föregående frame
    private bool wasJumping = false; // Håller koll på om spelaren hoppade tidigare

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player Transform inte satt i CameraTarget!");
            return;
        }

        playerController = player.GetComponent<RatchetController>();
        if (playerController == null)
        {
            Debug.LogError("Ingen RatchetController hittades på spelaren!");
        }

        targetY = transform.position.y;
        targetPosition = new Vector3(player.position.x, targetY, player.position.z);

        lastPlayerY = player.position.y; // Spara startvärdet
    }

    void Update()
    {
        if (player == null || playerController == null) return;

        CheckGroundStatus();

        bool isJumping = playerController.IsJump;
        bool isGlide = playerController.ISHelikopter;
        bool isFallingOrSliding = player.position.y < lastPlayerY; // Spelaren rör sig neråt

        // Om spelaren är på marken, återställ wasJumping
        if (isGrounded)
        {
            wasJumping = false;
        }

        // Uppdatera targetY endast om:
        // - Spelaren är på marken
        // - Spelaren glider
        // - Spelaren faller men har INTE hoppat nyligen
        if (isGrounded || isGlide || (!wasJumping && isFallingOrSliding))
        {
            targetY = Mathf.Lerp(targetY, player.position.y, Time.deltaTime * ySmoothSpeed);
           
        }
        // Uppdatera kamerans position utan att ändra targetY under hopp
        targetPosition = new Vector3(player.position.x, targetY, player.position.z);

        
            transform.position = targetPosition;
        
        // Spara spelarens Y-position för nästa frame
        lastPlayerY = player.position.y;


       

      
        // Om spelaren hoppar, sätt wasJumping till true
        if (isJumping)
        {
            wasJumping = true;
        }
    }








    void CheckGroundStatus()
    {
        // Raycast neråt från spelaren för att kolla om den är på marken
        isGrounded= Physics.Raycast(
            player.position + Vector3.up * 0.1f, // Starta lite ovanför för att undvika kollision med spelarens egen collider
            Vector3.down,
            0.2f,                        // Kontrollera kort avstånd neråt
            groundLayer
        );

        // Debug ray för att se i editorn
        Debug.DrawRay(player.position + Vector3.up * 0.1f, Vector3.down * 0.2f, Color.red);
    }
}