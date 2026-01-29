using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MissionFly : MonoBehaviour
{

    public List<GameObject> DropShips;

    bool ISOn = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        for(int i = 0; i < DropShips.Count; i++)
        {
           
            if(!MissionSound.MissionSound_.sound.isPlaying)
            {
                if (DropShips[i] == null)
                {
                    DropShips.Remove(DropShips[i]);

                    MissionSound.MissionSound_.i++;
                    MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);

                }
            }
               
           


            
        }



    }
}
