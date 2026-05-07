using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentBolt : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            GameObject.FindObjectOfType<Player>().anime.SetBool("Water", true);

            GameObject.FindObjectOfType<Water>().isTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water")
        {
            GameObject.FindObjectOfType<Player>().anime.SetBool("Water", false);
            GameObject.FindObjectOfType<Water>().isTrigger = false;
        }
    }


    public void getbolt()
    {
        GameObject.FindObjectOfType<BoltGet>().getbolt();


    }

    public void disableparent()
    {
        GameObject.FindObjectOfType<BoltGet>().disableparent();
    }
}
