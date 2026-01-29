using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBlue : MonoBehaviour
{

    public float MoveSpeed;
    public float MoveTime;

    float StartTime;
    public int Index = 0;
    bool ismove = true;
    // Start is called before the first frame update
    void Start()
    {
        StartTime = MoveTime;
    }

    // Update is called once per frame
    void Update()
    {


        MoveTime -= Time.deltaTime;

        if(MoveTime < 0)
        {

           

            if (ismove)
            {

                if (Index == 0)
                {
                    Index++;
                }
                
                ismove = false;
            }
            else
            {
                ismove = true;
                if (Index == 1)
                {
                    Index--;
                }
            }

            MoveTime = StartTime;
          
        }


      

        if (Index == 0)
        {
            transform.position += Vector3.right * MoveSpeed * Time.deltaTime;
        }
        if(Index == 1)
        {
            transform.position += -Vector3.right * MoveSpeed * Time.deltaTime;
        }
    }
}
