using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnMission : MonoBehaviour
{
    public Transform SpawnPoint;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {

      
        player.GetComponentInChildren<CharacterController>().enabled = false;
        player.transform.position = SpawnPoint.position;
        player.GetComponentInChildren<CharacterController>().enabled = true;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
