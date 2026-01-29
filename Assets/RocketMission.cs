using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RocketMission : MonoBehaviour
{

    public List<GameObject> Rockets;
    public List<GameObject> Enemies;
    public List<GameObject> DropShip;
    public static RocketMission RocketMission_;
    public int Index = 0;
    public int MaxIndex = 0;
   

    bool iss = false;
    // Start is called before the first frame update
    void Start()
    {
        RocketMission_ = GetComponent<RocketMission>();
    }

    // Update is called once per frame
    void Update()
    {
        

        if(!MissionSound.MissionSound_.sound.isPlaying)
        {
            if (Rockets.Count < 2)
            {
                if (5 > MissionSound.MissionSound_.i)
                {
                    MissionSound.MissionSound_.i++;
                    MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
                }
                
            }

            if(Rockets.Count == 0)
            {
                if (Enemies.Count == 0)
                {
                    if (DropShip.Count == 0)
                    {

                        if (8 > MissionSound.MissionSound_.i)
                        {
                            MissionSound.MissionSound_.i++;
                            MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
                        }

                    }
                   

                }

               
            }


           

           
        }
        else
        {
            if(MissionSound.MissionSound_.i == 8)
            {
                EnemiesMission.EnemiesMission_.IsWin = true;
            }
        }
      
      
        
        
           
        

      





    }
}
