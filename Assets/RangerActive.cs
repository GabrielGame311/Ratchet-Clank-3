using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerActive : MonoBehaviour
{

    public List<GameObject> Enemies;

    GameObject[] rangers;
    public bool IsSound;
    public bool ContinueMoving = false;
    public int enemies;

    public int Index = 0;
    public static RangerActive rangeractive_;

    // Start is called before the first frame update
    void Start()
    {
        rangeractive_ = GetComponent<RangerActive>();
    }

    // Update is called once per frame
    void Update()
    {
        rangers = GameObject.FindGameObjectsWithTag("Ranger");
        enemies = Enemies.Count;


        if(Enemies[Index] == null)
        {


            Index++;





            if (IsSound)
            {
                MissionSound.MissionSound_.i++;
                MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);

                IsSound = false;
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

           // Destroy(Enemies[Index].gameObject);
           

        }


        if(Enemies[Index].GetComponentsInChildren<EnemiesHealth>().Length == 0)
        {
            Destroy(Enemies[Index].gameObject);
        }

    }

  
}
