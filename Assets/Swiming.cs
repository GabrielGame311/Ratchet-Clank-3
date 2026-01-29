using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swiming : MonoBehaviour
{
    public bool exittime = false;
    public bool Ontrigger = false;
    private Animator anime;
    public GameObject SwimingObj;
    public AudioSource sound;
    public GameObject helpcanvas;
    public bool Canvas = true;
    public Animator anime2;
    private void Start()
    {
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();

        
    }




    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            if(exittime == true)
            {
                collision.collider.GetComponent<RatchetController>().gravity = true;
                collision.collider.GetComponent<RatchetController>().isSwimming = false;
                anime.SetBool("Swiming", false);
                SwimingObj.SetActive(false);
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
       
        
        
        
        if(Ontrigger == true)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<RatchetController>().gravity = false;
                other.GetComponent<RatchetController>().isSwimming = true;
                anime.SetBool("Swiming", true);
                SwimingObj.SetActive(true);

                if (Canvas == true)
                {
                    StartCoroutine(wait());
                }


            }
        }

        if(exittime == true)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<RatchetController>().gravity = true;
                other.GetComponent<RatchetController>().isSwimming = false;
                anime.SetBool("Swiming", false);
                SwimingObj.SetActive(false);
            }
        }
    }

    IEnumerator wait()
    {
        Canvas = false;
        sound.Play();
        helpcanvas.SetActive(true);
        yield return new WaitForSeconds(12.5f);
        anime2.SetTrigger("Trigger");

       

        yield return new WaitForSeconds(1);
        helpcanvas.SetActive(false);

    }
}
