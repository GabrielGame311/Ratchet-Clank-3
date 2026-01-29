using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropShipsActive : MonoBehaviour
{

    public List<GameObject> enemies = new List<GameObject>();
    public int currentEnemyIndex = 0;
   


    // Start is called before the first frame update
    void Start()
    {
       

    }

    // Update is called once per frame
    void Update()
    {
       
        if (enemies[currentEnemyIndex] == null)
        {
            // Set the next enemy as current
            currentEnemyIndex++;

            // Check if there are any more enemies in the list
            if (currentEnemyIndex >= enemies.Count)
            {
                // No more enemies in the list, do something else
                return;
            }

            // Activate the next enemy
           enemies[currentEnemyIndex].gameObject.SetActive(true);





            MissionSound.MissionSound_.i = currentEnemyIndex;
            MissionSound.MissionSound_.Mission4(currentEnemyIndex);
        }
        else
        {
            enemies[currentEnemyIndex].SetActive(true);
        }


      
          
        

    }
}
