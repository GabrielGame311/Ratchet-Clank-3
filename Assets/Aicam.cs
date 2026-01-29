using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aicam : MonoBehaviour
{
    public float Speed = 5f;           // Movement speed of the camera
    public float RotateSpeed = 2f;     // Speed at which the camera rotates to look at the player
    public Vector3 Offset = new Vector3(0, 5, -10); // Position offset from the player
  
    public float StopDistance = 10f;   // Distance after which the camera stops following
    public Vector3 StopPosition;       // Position after which the camera stops following the player
        // Time after which camera will resume following after stopping

    private GameObject Player_;        // Reference to the player
    private bool followPlayer = true;  // Flag to determine if the camera is following the player
    private float followTimer = 0f;    // Timer to track how long the camera has been following
    private Vector3 initialPosition;   // Initial position of the player
    private float cooldownTimer = 0f;  // Cooldown timer to wait before resuming follow
    public float PlayerDistance;
    bool isPos = true;

    // Start is called before the first frame update
    void Start()
    {
        Player_ = GameObject.FindGameObjectWithTag("Player");
       
    }

    // Update is called once per frame
    void Update()
    {

        float dis = Vector3.Distance(transform.position, Player_.transform.position);

        if(dis < PlayerDistance)
        {
            if(isPos)
            {
                initialPosition = Player_.transform.position;
                isPos = false;
            }

            if (Player_ != null)
            {
                // Log player position and follow conditions for debugging
                Debug.Log("Player Position: " + Player_.transform.position);

                // Time-based stop condition


                // Distance-based stop condition
                float distanceFromStart = Vector3.Distance(Player_.transform.position, initialPosition);
                Debug.Log("Distance From Start: " + distanceFromStart);

                if (followPlayer)
                {
                    // Follow the player with smooth movement
                    Vector3 targetPosition = Player_.transform.position + Offset;
                    transform.position = Vector3.Lerp(transform.position, targetPosition, Speed * Time.deltaTime);

                    // Rotate smoothly to look at the player
                    Vector3 directionToPlayer = (Player_.transform.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);

                    // Stop following after time or distance condition is met
                    if (distanceFromStart >= StopDistance)
                    {
                        Debug.Log("Stop Following: Condition Met");
                        followPlayer = false;  // Stop following the player

                    }
                }
                else
                {
                    Vector3 directionToPlayer = (Player_.transform.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                }


            }
        }


      
    }
}
