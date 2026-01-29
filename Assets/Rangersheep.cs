using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rangersheep : MonoBehaviour
{

    public Animator anime;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            anime.gameObject.SetActive(true);
            anime.SetBool("Drive", true);
            gameObject.SetActive(false);
        }
    }
}
