using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendorTriggerActive : MonoBehaviour
{
    Animator anime;

    // Start is called before the first frame update
    void Start()
    {
        anime = GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bolt")
        {
            

          
                anime.SetBool("Hide", true);
            
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Bolt")
        {
           
                anime.SetBool("Hide", false);
            

               
        }
    }
}
