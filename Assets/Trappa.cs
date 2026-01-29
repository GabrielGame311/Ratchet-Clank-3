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
        if (isTrappa)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                // Set climbing animation
                anime.SetBool("Trappa", true);

                // Move player up the ladder
                player.transform.position += Vector3.up * Time.deltaTime * speed;

                // Check if the player has reached the top of the ladder
                if (ladderTopY > 0 && player.transform.position.y >= ladderTopY)
                {
                    ExitLadder();
                }
            }
            else
            {
                // Stop climbing animation if Space is not pressed
                anime.SetBool("Trappa", false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trappa"))
        {
            isTrappa = true;
            trappaList.Add(other.gameObject);
            controller.enabled = false;
           
            
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Trappa"))
        {
            ExitLadder();
        }
    }

    private void ExitLadder()
    {
        // Reset states when leaving the ladder
        isTrappa = false;

        controller.enabled = true;
        anime.SetBool("Trappa", false);
    }
}