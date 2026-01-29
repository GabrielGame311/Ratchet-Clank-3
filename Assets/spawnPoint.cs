using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnPoint : MonoBehaviour
{

    GameObject player_;
    public static spawnPoint instance;

    public int spawnID;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        player_ = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SpawnPlayer(int id)
    {

        spawnID = id;
        player_.GetComponent<CharacterController>().enabled = false;
        player_.transform.position = transform.position;

        player_.GetComponent<CharacterController>().enabled = true;

    }

}
