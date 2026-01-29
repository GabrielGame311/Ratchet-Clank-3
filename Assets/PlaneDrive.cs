using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneDrive : MonoBehaviour
{
    public float DistancePlayer = 10f;  // Distance at which the object starts moving
    public float Speed = 5f;           // Movement speed
    public float TimeCountStart = 3f;  // Duration of forward and backward movement
    public float startTimeWait = 1f;   // Pause before and between movements
    public int InitialDirection = 0;  // 0 for forward first, 1 for backward first

    private float TimeCount;           // Internal time tracker for movement
    private float WaitTime;            // Internal wait time tracker
    private bool isForward;            // Tracks the current movement direction
    private bool isWaiting;            // Tracks if the object is in a waiting phase

    GameObject Player_;

    // Start is called before the first frame update
    void Start()
    {
        Player_ = GameObject.FindGameObjectWithTag("Player");

        // Set initial direction based on InitialDirection
        if (InitialDirection == 0)
        {
            TimeCount = TimeCountStart; // Start by moving forward
            isForward = true;
        }
        else if (InitialDirection == 1)
        {
            TimeCount = -TimeCountStart; // Start by moving backward
            isForward = false;
        }

        isWaiting = true;               // Start with a wait
        WaitTime = startTimeWait;       // Initial wait time
    }

    // Update is called once per frame
    void Update()
    {
        float dis = Vector3.Distance(transform.position, Player_.transform.position);

        if (dis < DistancePlayer)
        {
            if (isWaiting)
            {
                WaitTime -= Time.deltaTime; // Countdown the wait time
                if (WaitTime <= 0)
                {
                    isWaiting = false;     // End waiting phase
                }
            }
            else
            {
                if (isForward)
                {
                    TimeCount -= Time.deltaTime; // Decrease time counter for forward movement

                    if (TimeCount > 0) // Move forward
                    {
                        transform.position += transform.forward * Speed * Time.deltaTime;
                    }
                    else
                    {
                        // End forward movement, switch to backward
                        TimeCount = -TimeCountStart; // Reset for backward movement
                        isForward = false;
                        isWaiting = true;            // Start waiting before backward movement
                        WaitTime = startTimeWait;    // Reset wait time
                    }
                }
                else
                {
                    TimeCount += Time.deltaTime; // Increase time counter for backward movement

                    if (TimeCount < 0) // Move backward
                    {
                        transform.position += -transform.forward * Speed * Time.deltaTime;
                    }
                    else
                    {
                        // End backward movement, switch to forward
                        TimeCount = TimeCountStart; // Reset for forward movement
                        isForward = true;
                        isWaiting = true;           // Start waiting before forward movement
                        WaitTime = startTimeWait;   // Reset wait time
                    }
                }
            }
        }
    }
}
