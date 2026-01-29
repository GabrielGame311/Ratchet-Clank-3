using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive1 : MonoBehaviour
{

    public float DriveSpeed;
    public Transform Point;
    Vector3 startPos;
    GameObject Player;

    public float WaitTime;
    float StartTime;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        StartTime = WaitTime;
    }


    bool IsDrive = false;

    // Update is called once per frame
    void Update()
    {

        
        if(IsDrive)
        {


            transform.position = Vector3.MoveTowards(transform.position, Point.transform.position, DriveSpeed * Time.deltaTime);


            if(transform.position == Point.transform.position)
            {
                WaitTime -= Time.deltaTime;
                Player.GetComponent<CharacterController>().enabled = true;
                if (WaitTime < 0)
                {


                    Player.GetComponent<CharacterController>().enabled = false;

                    IsDrive = false;
                    WaitTime = StartTime;



                }
            }


          
        }
        else
        {


            transform.position = Vector3.MoveTowards(transform.position, startPos, DriveSpeed * Time.deltaTime);


            if (transform.position == startPos)
            {
                Player.GetComponent<CharacterController>().enabled = true;

                WaitTime -= Time.deltaTime;
                if (WaitTime < 0)
                {


                    Player.GetComponent<CharacterController>().enabled = false;

                    IsDrive = true;
                    WaitTime = StartTime;



                }
            }


        }
      
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Player = other.gameObject;
            Player.transform.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            Player.transform.parent = null;
            Player = null;
            Player.GetComponent<CharacterController>().enabled = true;
        }
    }




}
