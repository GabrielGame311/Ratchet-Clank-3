using System.Collections;
using UnityEngine;

public class DrivePhinix : MonoBehaviour
{
    // Reference to the player
    GameObject Player_;
    public Animator anime; // Animator for the doors

    // Determines if the object is triggered to move
    public bool IsTrigger = false;

    // Array of points for movement
    public Transform[] Points;
    Vector3 targetPosition;

    // Current index of the target point
    public int Index = 0;

    // Speed of movement
    public float Speed = 5f;
    bool ItsMoving = false;

    private void Start()
    {
        // Find the player object with the "Player" tag
        Player_ = GameObject.FindGameObjectWithTag("Player");

        // Initially, close the back door and open the front door

        CloseFrontDoor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player_.transform.SetParent(transform);
            IsTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsTrigger = false;
            Player_.transform.parent = null;
        }
    }

    private void Update()
    {
        if (IsTrigger)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Disable player movement
                Player_.GetComponent<CharacterController>().enabled = false;

                // Close the back door
                CloseBackDoor();

                // Update the target index (toggle between points)
                Index = (Index + 1) % Points.Length; // Loop through points

                // Start movement
                StartCoroutine(WaitAndMove());
                IsTrigger = false;
            }
        }
    }

    private void FixedUpdate()
    {
        // If the trigger is active, move the object toward the target point
        if (ItsMoving)
        {
            MoveToPoint();
        }
    }

    void MoveToPoint()
    {
        // Get the target point position
        targetPosition = Points[Index].position;

        // Move the object toward the target point
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);

        // Stop moving once the target point is reached
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            ItsMoving = false; // Stop movement once the target is reached

            // After reaching the point, close the front door and open the new one
           
            OpenFrontDoor();

            // Re-enable player movement
            Player_.GetComponent<CharacterController>().enabled = true;
        }
    }

    IEnumerator WaitAndMove()
    {
        // Wait a bit before starting the movement
        yield return new WaitForSeconds(1f);
        ItsMoving = true;
    }

    void CloseBackDoor()
    {
        if (Index == 0)
        {
            
            anime.SetBool("Door1", true); // Close Door1 if moving from index 0
           
        }
        if (Index == 1)
        {

           
            anime.SetBool("Door2", true); // Close Door2 if moving from index 1

        }
    }

    void OpenFrontDoor()
    {
        if (Index == 0)
        {
           
            anime.SetBool("Door1", false); // Open Door1 at index 0
            
        }
        if (Index == 1)
        {
           
            anime.SetBool("Door2", false); // Open Door2 at index 1
        }
    }

    void CloseFrontDoor()
    {
        if (Index == 0)
        {
            
            anime.SetBool("Door2", true); // Close Door1 when moving away from index 0
        }
        if (Index == 1)
        {
            
            anime.SetBool("Door1", true); // Close Door2 when moving away from index 1
        }
    }
}
