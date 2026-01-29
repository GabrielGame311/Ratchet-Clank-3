using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMissionSpawn : MonoBehaviour
{

    public int Mission;
    GameObject Player;

    RangerShip RangerShip_;


    // Start is called before the first frame update
    void Start()
    {
        GameObject.FindGameObjectWithTag("Player");
        RangerShip_ = GameObject.FindObjectOfType<RangerShip>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartMission()
    {
        GameObject.FindObjectOfType<CharacterController>().enabled = false;
        RangerShip_.EnableAllScriptsOnPlayer();
      //  Player.transform.position = GameObject.FindObjectOfType<RangerShip>().SpawnPoint.transform.position;
        
        GameObject.FindObjectOfType<CharacterController>().enabled = true;
    }
}
