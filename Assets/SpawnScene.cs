using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnScene : MonoBehaviour
{

    public Transform SpawnPoint;

    GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");

        Player.GetComponent<CharacterController>().enabled = false;
        Player.transform.position = SpawnPoint.transform.position;
        Player.transform.LookAt(SpawnPoint.transform);
        Player.GetComponent<CharacterController>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
