using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalacticRangersActive : MonoBehaviour
{

    public GameObject[] Enemies;
    public int Index = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Enemies[Index] == null)
        {
            Index++;



           
        }


        if (Enemies[Index].GetComponentsInChildren<EnemiesHealth>().Length == 0)
        {
            foreach (GalacticRangers ranger in GameObject.FindObjectsOfType<GalacticRangers>())
            {
                ranger.isMoving = true;

                Destroy(Enemies[Index]);
                //Index++;
            }
        }
    }
}
