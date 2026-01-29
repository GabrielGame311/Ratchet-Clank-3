using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    Animator anime;


    bool IsTrigger = false;
    // Start is called before the first frame update
    void Start()
    {
        anime = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(anime != null)
            {
                IsTrigger = true;
                if (IsTrigger)
                {
                   
                    anime.SetTrigger("Trigger");
                    IsTrigger = false;

                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            IsTrigger = false;
        }

           
    }
}
