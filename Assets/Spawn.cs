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
        GameObject.FindObjectOfType<CharacterController>().enabled = false;
        Player.transform.position = transform.position;
        GameObject.FindObjectOfType<CharacterController>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ship()
    {
        anime.SetTrigger("Ship");
    }
}
