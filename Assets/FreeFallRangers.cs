using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFallRangers : MonoBehaviour
{

    public GalacticRangers[] Rangers;
    public float MoveSpeed;
    public float FlyTime;
    public float Speed;
    public int Place;
    float startTime;
    public GameObject[] RangerDie;
    public bool FreeFall = false;
    Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        


        pos = transform.position;

        startTime = FlyTime;
    }


  
    

    // Update is called once per frame
    void Update()
    {
        Rangers = GetComponentsInChildren<GalacticRangers>();
        if (FreeFall)
        {
            transform.position += Vector3.down * Speed * Time.deltaTime;
            foreach (GalacticRangers ranger in Rangers)
            {
                
                ranger.HeadAnime.SetBool("FreeFall", true);
                ranger.FootAnime.SetBool("FreeFall", true);

                FlyTime -= Time.deltaTime;

                if (FlyTime < 0)
                {

                    FlyTime = startTime;

                    Place++;
                     

                }

                float flytimestart = 0;

                if (FlyTime > flytimestart)
                {

                   
                    if (Place == 1)
                    {
                        transform.position += Vector3.forward * MoveSpeed * Time.deltaTime;
                    }
                    if (Place == 2)
                    {
                        transform.position += Vector3.left * MoveSpeed * Time.deltaTime;
                    }

                    if (Place == 3)
                    {
                        transform.position -= Vector3.left * MoveSpeed * Time.deltaTime;
                    }


                    if (Place == 4)
                    {
                        transform.position -= Vector3.forward * MoveSpeed * Time.deltaTime;
                    }


                }
                if (Place == 5)
                {
                    Place = 0;
                }
            }

            

        }
        else
        {

            foreach (GalacticRangers ranger in Rangers)
            {
                ranger.HeadAnime.SetBool("FreeFall", false);
                ranger.FootAnime.SetBool("FreeFall", false);

            }



        }



    }
}
