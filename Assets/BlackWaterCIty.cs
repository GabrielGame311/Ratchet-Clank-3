using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackWaterCIty : MonoBehaviour
{

    public int ContinuePlayto;






    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!MissionSound.MissionSound_.sound.isPlaying)
        {

            if(MissionSound.MissionSound_.i > 1)
            {
               // ContinuePlayto++;

                if (0 > ContinuePlayto)
                {
                    MissionSound.MissionSound_.i++;
                }
            }



        }
    }
}
