using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowTransfer : MonoBehaviour
{

    public Transform[] FlyPoint;
    public int Point;
    GameObject Player;
    public bool IsTrigger = false;
    public bool Transporting = false;
    public float TransportSpeed;
    public int Count;
    public float RotateSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(IsTrigger)
        {

             
            if(Input.GetKeyDown(KeyCode.E))
            {
                StartTransport();
                IsTrigger = false;
                
            }
        }

        if (Transporting)
        {

           



            Vector3 direction = (FlyPoint[Point].position - transform.position).normalized;
            // Calculate the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate towards the target point
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime * 360);

            // Check if the player is facing the target direction
            float angle = Quaternion.Angle(transform.rotation, targetRotation);
            if (angle < 1f) 
            {

                transform.position = Vector3.MoveTowards(transform.position, FlyPoint[Point].position, TransportSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, FlyPoint[Point].position) < 0.1f)
                {
                    // Move to the next point
                    if (Count == 0)
                    {
                        Point++;
                    }
                    if (Count == 1)
                    {
                        Point--;
                    }

                    // If we've reached the last point, stop transporting
                    if (Point >= FlyPoint.Length)
                    {
                       
                        if(Count == 0)
                        {
                            Count = 1;
                        }
                        else if(Count == 1)
                        {
                            Count = 0;
                        }
                           
                        if(Point < FlyPoint.Length)
                        {
                            Point++;
                        }
                        else
                        {
                            Point++;
                        }
                       
                            //Count = 0;
                        
                        Player.GetComponent<CharacterController>().enabled = true;
                        Player.transform.parent = null;
                        Transporting = false;
                    }
                }
            } // If the angle difference is small enough, start moving 

            
        }
    }

   


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            IsTrigger = true;
            Player = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            IsTrigger = false;
            Player = null;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Player")
        {
            IsTrigger = true;
            Player = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            IsTrigger = false;
            Player = null;
        }
    }

    public void StartTransport()
    {
        Transporting = true;
        Player.GetComponent<CharacterController>().enabled = false;
        Player.transform.SetParent(transform);
        Player.GetComponent<RatchetController>().Ground = true;

    }
}
