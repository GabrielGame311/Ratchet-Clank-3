using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missions : MonoBehaviour
{

    public GameObject[] MissionsList;
    public int Mission;
    public static Missions Mission_;
    int mis = 0;
    public bool IsMission = false;
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        Mission_ = GetComponent<Missions>();

       
    }

    // Update is called once per frame
    void Update()
    {
        

       // BattleMission_UI.BattleMission.MissionsList = Mission;
        

        if(IsMission)
        {

        }

        




    }


    public void MissionsSet(int miss)
    {
        IsMission = true;
        Mission = miss;
        MissionsList[miss].SetActive(true);
      

        

    }

}
