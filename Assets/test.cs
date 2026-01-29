using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{

    public GameObject player;
    public GameObject[] spawnpoint;
    public GameObject timeline;
    public Animator anime;
    public Movment gravity;
    public freefall freefalgravity;

    public GameObject playerfall;
    public GameObject playernow;

    public GameObject FirstMusicOff;
    public GameObject SecondMusicOn;


    void OnTriggerEnter(Collider other)
    {

        playerfall.SetActive(true);
        playernow.SetActive(false);
        timeline.SetActive(true);
        FirstMusicOff.SetActive(false);
        SecondMusicOn.SetActive(true);

       



    }


   

}
