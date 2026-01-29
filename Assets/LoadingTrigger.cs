using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingTrigger : MonoBehaviour
{

    public GameObject LoadingScene;
    private GameObject player;

    bool trigger = false;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }



    private void Update()
    {
        if(trigger == true)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                player.SetActive(false);
                LoadingScene.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        trigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        trigger = false;
    }
}
