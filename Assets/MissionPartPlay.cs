using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionPartPlay : MonoBehaviour
{
    // Start is called before the first frame update

    bool isplaying = true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isplaying)
        {
            if (GetComponentsInChildren<EnemiesHealth>().Length < 3)
            {
                MissionSound.MissionSound_.i++;
                MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
                isplaying = false;
            }
        }
    }
}
