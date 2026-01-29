using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniThyrraShip : MonoBehaviour
{

    

    Animator anime;



    public float moveSpeed = 3f; // Speed of movement
    private float leftDuration = 3f; // Duration of leftward movement
    private float rightDuration = 3f; // Duration of rightward movement
    private float leftTimer; // Timer for leftward movement
    private float rightTimer; // Timer for rightward movement
    private bool isMovingLeft = true; // Flag to track the movement direction

    void Start()
    {
        leftTimer = leftDuration; // Start the left timer
    }

    void Update()
    {
        if (isMovingLeft)
        {
            leftTimer -= Time.deltaTime;

            if (leftTimer < 0)
            {
                // Switch to rightward movement
                isMovingLeft = false;
                rightTimer = rightDuration; // Start the right timer
            }

            // Move left
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;
        }
        else
        {
            rightTimer -= Time.deltaTime;

            if (rightTimer < 0)
            {
                // Switch to leftward movement
                isMovingLeft = true;
                leftTimer = leftDuration; // Start the left timer
            }

            // Move right
            transform.position -= Vector3.left * moveSpeed * Time.deltaTime;
        }
    }

}
