using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGadget : MonoBehaviour
{

    bool isTrigger = false;
    public int SpawMission = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isTrigger)
        {

            if(Input.GetKeyDown(KeyCode.E))
            {
                spawnPoint.instance.SpawnPlayer(SpawMission);
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            isTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isTrigger = false;
        }
    }
}
