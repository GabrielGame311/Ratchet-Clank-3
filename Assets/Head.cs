using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AmmoBox"))
        {
            other.GetComponent<AmmoCrate>().Break(2);
        }
        if (other.CompareTag("Box"))
        {
            other.GetComponent<Box>().Break(2);
        }
        if (other.CompareTag("Health"))
        {
            other.GetComponent<HealthItem>().Break(2);

        }
    }


}
