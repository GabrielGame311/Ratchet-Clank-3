using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dorr : MonoBehaviour
{

    public Animator anime;

    public AudioSource sound;


    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Player")
        {
            sound.Play();
            anime.SetBool("Door", true);
        }

       
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.tag == "Player")
        {
            sound.Play();
            anime.SetBool("Door", false);
        }

       
    }
}
