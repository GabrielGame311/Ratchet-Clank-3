using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameSceneAquatos : MonoBehaviour
{

    public GameObject timeline;

    public EnemiesHealth[] enemies;
   
    public  bool hack = false;
    public bool door = true;

    public bool enemieshealth = false;
    
    
    
    private void Update()
    {
        if (hack == true)
        {
            timeline.SetActive(true);
        }


        if(enemieshealth == true)
        {

            

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].destroy == true)
                {





                    hack = true;

                }
            }


           
        }
       

        
    }




    private void OnTriggerEnter(Collider other)
    {

       

        if (other.CompareTag("Player"))
        {

            if (door == true)
            {
                timeline.SetActive(true);
            }


        }
    }

   
}
