using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject canvas;
    public Transform SpawnPoint;
    public RatchetController movment;
    public CharacterController controller;
    public GameObject player;
    public Animator anime;
    public ParticleSystem particle;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }



    private void OnTriggerStay(Collider other)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

            if(other.tag == "Player")
            {
                StartCoroutine(Delayteleport());
            }



        }



        
    }

    IEnumerator Delayteleport()
    {
        particle.Play();
        anime.SetTrigger("Teleport");
        controller.enabled = false;
        movment.enabled = false;
       
        yield return new WaitForSeconds(2f);
        player.transform.position = SpawnPoint.transform.position;
        
        controller.enabled = true;
        movment.enabled = true;

    }
   



    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            canvas.SetActive(true);
            
        }   
       
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            canvas.SetActive(false);
        }
    }

}
