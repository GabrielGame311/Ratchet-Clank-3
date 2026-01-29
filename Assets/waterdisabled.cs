using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterdisabled : MonoBehaviour
{
    public GameObject waterscreen;

    public Animator anime;


    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player")
        {
            waterscreen.SetActive(false);




        }

    }
}
