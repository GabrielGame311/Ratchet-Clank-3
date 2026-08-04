using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{

    public Animator anime;
    public GameObject Player;
    
    
    // Start is called before the first frame update
    void Start()
    {

           Player = GameObject.FindWithTag("Player");

            Player.GetComponent<CharacterController>().enabled = false;
            Player.transform.position = transform.position;
            Player.GetComponent<CharacterController>().enabled = true;
    }




    // Update is called once per frame
    
    public void ship()
    {
        anime.SetTrigger("Ship");
    }
}
