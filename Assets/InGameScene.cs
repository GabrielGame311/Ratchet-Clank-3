using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameScene : MonoBehaviour
{

    public GameObject laser;
    public GameObject player;
    public Animator anime;
    public GameObject TimeLine;
    private void Start()
    {
        anime.SetBool("Crouch", true);
    }




    private void OnTriggerEnter(Collider other)
    {

        if(other.tag == "Player")
        {
            anime.SetBool("Crouch", false);
            TimeLine.SetActive(true);
            StartCoroutine(hide());
        }

       



    }

    IEnumerator hide()
    {
        yield return new WaitForSeconds(1.4f);
        Destroy(laser);
    }




}
