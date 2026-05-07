using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    public GameObject waterscreen;

    public Animator anime;

    public GameObject Fish_;

    public float count;
    public GameObject player;
    public bool isTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
     
        if(other.tag == "Armor") 
        {

            //waterscreen.SetActive(true);
            
            isTrigger = true;


        }
        
    }


    private void OnTriggerExit(Collider other)
    {

        if (other.tag == "Armor")
        {

            isTrigger = false;






        }
    }

   

    private void Update()
    {
        if(isTrigger)
        {

            count -= Time.deltaTime;
            if(count < 0)
            {

                count = 1000;
                Instantiate(Fish_, player.transform.position, player.transform.rotation);
                isTrigger = false;
            }
        }
    }

}
