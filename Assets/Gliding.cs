using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gliding : MonoBehaviour
{

    public Animator anime;
    RatchetController controller;
    bool dead = false;
    // Start is called before the first frame update
    void Start()
    {
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<RatchetController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
       
        dead = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Glitch"))
        {
            anime.SetBool("Glitch", true);
            controller.collision = true;
            dead = true;

            if (dead == true)
            {
                StartCoroutine(wait());
            }
        }

        

    }


    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Glitch"))
        {
            anime.SetBool("Glitch", false);
            dead = false;
            controller.collision = false;
        }
    }



   


    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Glitch"))
        {
            anime.SetBool("Glitch", true);
            controller.collision = true;
            dead = true;

            if (dead == true)
            {
                StartCoroutine(wait());
            }
        }

        
       
    }

    

    
    private void OnCollisionExit(Collision other)
    {
        if (other.collider.CompareTag("Glitch"))
        {
            anime.SetBool("Glitch", false);
            dead = false;
            controller.collision = false;

        }



    }


    IEnumerator wait()
    {
        
        yield return new WaitForSeconds(6);
        

        if (dead == true)
        {
            GameObject.FindObjectOfType<Player>().GetComponent<Player>().Deads();
        }

        dead = false;
        
    }
}
