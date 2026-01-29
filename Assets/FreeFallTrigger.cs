using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFallTrigger : MonoBehaviour
{
    public GameObject player;
    public Transform SpawnPlayer;



    public float Pos;
    public GameObject EnemiesActive;
    public static FreeFallTrigger freefallTrigger;
    bool ISplaying = true;

    private void Start()
    {
        freefallTrigger = GetComponent<FreeFallTrigger>();
        //player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && freefallTrigger != null)
        {
            // Disable the CharacterController (if necessary)
            CharacterController characterController = player.GetComponentInChildren<CharacterController>();
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            // Set the player's position to the spawn point
            player.transform.position = SpawnPlayer.position;

            // Re-enable the CharacterController (if necessary)
            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }
        else
        {
            Debug.LogError("Player or FreeFallTrigger not found. Make sure you have set them up correctly.");
        }
    }


    // Update is called once per frame
    void Update()
    {

      

        if(freefall.Freefall.ItsFalling)
        {
            

            player.transform.rotation = Quaternion.LookRotation(SpawnPlayer.forward);
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            if(IOSController.IosController_ != null)
            {
                IOSController.IosController_.RightIos_.SetActive(false);
            }

            freefall.Freefall.ItsFalling = true;
           
            player.GetComponentInChildren<RatchetController>()._directionY = 0;
            player.GetComponentInChildren<RatchetController>().Gravity = 0;
            //player.GetComponent<Animator>().applyRootMotion = true;
        }
        else if (other.tag == "Ranger")
        {
            GameObject.FindObjectOfType<FreeFallRangers>().FreeFall = true;
            foreach(GalacticRangers gl in GameObject.FindObjectOfType<FreeFallRangers>().Rangers)
            {
                gl.GetComponent<Rigidbody>().useGravity = false;
            }


        }


    }

   

    private void OnTriggerExit(Collider other)
    {

        if (other.tag == "Player")
        {

            GameObject.FindObjectOfType<MissionSound>().IsPlay = true;

            if (IOSController.IosController_ != null)
            {
                IOSController.IosController_.RightIos_.SetActive(true);
            }

                
            freefall.Freefall.ItsFalling = false;
            
            player.GetComponentInChildren<RatchetController>().Gravity = 9;
            EnemiesActive.SetActive(true);
            Destroy(gameObject, 2);
           
        }
        else if (other.tag == "Ranger")
        {
            GameObject.FindObjectOfType<FreeFallRangers>().FreeFall = false;

            foreach (GalacticRangers gl in GameObject.FindObjectOfType<FreeFallRangers>().Rangers)
            {
                gl.GetComponent<Rigidbody>().useGravity = true;
            }
        }


    }
}
