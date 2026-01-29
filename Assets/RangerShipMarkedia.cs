using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerShipMarkedia : MonoBehaviour
{
    public GameObject rangership;
    public GameObject ranger2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            
            rangership.gameObject.SetActive(true);
            ranger2.SetActive(false);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            ranger2.SetActive(true);
            rangership.gameObject.SetActive(false);
        }
    }
}
