using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{


    GameObject player;


    bool ISTrigger = false;

    public Transform TeleportPoint;





    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
        if(ISTrigger)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                player.GetComponent<CharacterController>().enabled = false;
                player.transform.position = TeleportPoint.position;
                player.GetComponent<CharacterController>().enabled = true;
            }
        }


    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            ISTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            ISTrigger = false;
        }
    }
}
