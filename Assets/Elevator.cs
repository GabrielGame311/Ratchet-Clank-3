using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public Transform Point1;
    public Transform Point2;
    public float MoveSpeed;
    public float WaitTime;
    GameObject player;
    float startTime;
    bool isMovingToPoint2 = true;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        startTime = WaitTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Player")
        {
            player.transform.parent = transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            player.transform.parent = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            player.transform.parent = transform;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
           player.transform.parent = null;
        }
    }


    private void FixedUpdate()
    {
        WaitTime -= Time.deltaTime;

        if (WaitTime < 0)
        {
            if (isMovingToPoint2)
            {
                MoveToPoint(Point2);
            }
            else
            {
                MoveToPoint(Point1);
            }

            if (WaitTime < -5) // After waiting for 5 seconds, reset the timer and toggle movement direction
            {
                WaitTime = startTime;
                isMovingToPoint2 = !isMovingToPoint2;
            }
        }
    }

    

   

    void MoveToPoint(Transform targetPoint)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, MoveSpeed * Time.deltaTime);
    }

}
