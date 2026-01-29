using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEnemies : MonoBehaviour
{

    public GameObject[] Enemies;

    GameObject[] rangers;
    public bool IsSound;
    public bool ContinueMoving = false;
    public int PointContinue;
    public bool trigger = false;

    public GameObject[] triggers;


    // Start is called before the first frame update
    void Start()
    {
        rangers = GameObject.FindGameObjectsWithTag("Ranger");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {


      
        if(other.tag == "RangerFall")
        {
            foreach (GameObject trigger in triggers)
            {
                trigger.SetActive(true);
            }
            Destroy(gameObject);
        }
        
       
        if(trigger == false)
        {
            if (other.tag == "Player")
            {
                foreach (GameObject enemie in Enemies)
                {
                    enemie.SetActive(true);
                }

                if (IsSound)
                {
                    MissionSound.MissionSound_.i++;
                    MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
                }


                foreach (GameObject ranger in rangers)
                {
                    ranger.GetComponent<GalacticRangers>().isMoving = true;


                    if (ContinueMoving)
                    {
                        ranger.GetComponent<GalacticRangers>().ContinueMove = true;
                    }
                    else
                    {
                        ranger.GetComponent<GalacticRangers>().ContinueMove = false;
                    }
                }


                trigger = true;


            }
        }
            
        


       

       
    }
}
