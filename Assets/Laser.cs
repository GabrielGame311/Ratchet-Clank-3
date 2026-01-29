using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Laser : MonoBehaviour
{
    public Thyrralaser thyrralaser1;
    public Animator anime;

    public VisualEffect laser1;

    public VisualEffect laser2;


    private void Start()
    {
        anime.SetBool("Attack2", true);
        thyrralaser1.enabled = true;
    }



    private void OnTriggerEnter(Collider other)
    {


        if (other.tag == "Player")
        {
            anime.SetBool("Attack2", true);
            thyrralaser1.enabled = true;

        }



        
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            anime.SetBool("Attack2", false);

            thyrralaser1.enabled = false;

        }

        
    }

  
    

}
