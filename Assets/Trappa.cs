using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trappa : MonoBehaviour
{
    public Animator anime;
   
    public float speed = 5f; // Climbing speed
    public float ladderTopY; // Y position to stop climbing (top of the ladder)
    private bool isTrappa = false; // Renamed for better readability
    private List<GameObject> trappaList = new List<GameObject>(); // Renamed for clarity
    private Transform player;
    CharacterController controller;
    
    
    void Start()
    {
        // Initialize components
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        controller = GameObject.FindObjectOfType<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        // Optionally set ladderTopY based on the ladder's top position
        // You can set this manually in the Unity Inspector or calculate it dynamically
    }

    void Update()
    {
       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trappa") && RatchetController.RatchetController_.climbCooldown <= 0 && RatchetController.RatchetController_._directionY <= 0.5f)
        {
          RatchetController.RatchetController_.isClimbing = true;
            RatchetController.RatchetController_._directionY = 0;
            RatchetController.RatchetController_.isGliding = false;
            RatchetController.RatchetController_.anime.SetBool("Run", false);
            //RatchetController.RatchetController_.transform.forward = -other.transform.forward;
        }

    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Trappa") && RatchetController.RatchetController_.climbCooldown <= 0 && RatchetController.RatchetController_._directionY <= 0)
            RatchetController.RatchetController_.isClimbing = true;
            RatchetController.RatchetController_.anime.SetBool("Run", false);
    }

    void OnTriggerExit(Collider other) { if (other.CompareTag("Trappa")) RatchetController.RatchetController_.isClimbing = false; }

    private void ExitLadder()
    {
        // Reset states when leaving the ladder
        isTrappa = false;

        controller.enabled = true;
        anime.SetBool("Trappa", false);
    }
}