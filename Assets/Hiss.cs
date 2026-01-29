using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hiss : MonoBehaviour
{

    public bool DriveHiss = false;
    public float Speed;
    public float TimeDrive;

    public Transform Point;
    GameObject player;
    Vector3 pos;
    public Transform SpawnPoint;
    bool IsSpawn = false;
    public AudioSource sound;
    public AudioClip Music;
    public bool ISTrigger = false;

    bool IsPressed = false;

    // Start is called before the first frame update
    void Start()
    {
        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(ISTrigger)
        {

            if (Input.GetKeyDown(KeyCode.E))
            {

                if (IsPressed)
                {
                    DriveHiss = false;
                    IsPressed = false;
                }
                else
                {
                    DriveHiss = true;
                    IsPressed = true;
                }

            }

        }

        if(IsSpawn == false)
        {
            if (transform.position == Point.position)
            {
                sound.clip = Music;
                sound.Play();
                player.GetComponent<CharacterController>().enabled = false;

                player.transform.position = SpawnPoint.transform.position;
                player.GetComponent<CharacterController>().enabled = true;
                IsSpawn = true;
            }
        }



    }


    private void FixedUpdate()
    {
        if (DriveHiss)
        {


            transform.position = Vector3.MoveTowards(transform.position, Point.transform.position, Time.deltaTime * Speed);


        }
        else
        {





            transform.position = Vector3.MoveTowards(transform.position, pos, Time.deltaTime * Speed);


        }
    }

    

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            player = other.gameObject;
            ISTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            ISTrigger = true;
        }

           
    }
}
