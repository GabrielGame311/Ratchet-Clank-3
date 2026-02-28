using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class EnemieActive : MonoBehaviour
{

    public GameObject[] Enemies;
    public EnemiesHealth[] health;

    public int EnemieIndex = 0;

    public bool Is4Enemies = false;
    public bool SoundOn = false;
    public int MissionWinPlay;
    public bool hasBeenAdded = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (Enemies[EnemieIndex].GetComponentsInChildren<EnemiesHealth>().Length == 0)
        {

          
            Destroy(Enemies[EnemieIndex].gameObject);
            Debug.Log("IsMoving");
            foreach (GalacticRangers gl in GalacticRangers.FindObjectsOfType<GalacticRangers>())
            {

                if(gl.targetPoint.Length == 1)
                {
                    gl.isMoving = true;
                   
                }

                
            }

            //EnemiesMission.instance.EnemiesList.Remove(Enemies[EnemieIndex].gameObject);


        }
    }

    // Update is called once per frame
    void Update()
    {

       
        if (!MissionSound.MissionSound_.sound.isPlaying)
        {
            if (Is4Enemies == false)
            {
                if (Enemies[EnemieIndex] == null)
                {
                    EnemieIndex++;



                    if (EnemieIndex > Enemies.Count())
                    {
                        return;
                    }

                    Enemies[EnemieIndex].SetActive(true);

                    MissionSound.MissionSound_.i++;
                    MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);


                }
            }

            if(MissionSound.MissionSound_.IsPlays == false)
            {
                if (SoundOn)
                {

                    Enemies[EnemieIndex].SetActive(true);
                    MissionSound.MissionSound_.i++;
                    MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);

                    SoundOn = false;
                }
            }

            if (!hasBeenAdded && MissionSound.MissionSound_.i == 1)
            {
                // Execute the code only once
                // ...
                FindObjectOfType<EnemieActive>().Is4Enemies = true;
                // Set the flag to true to indicate that the integer has been added
                hasBeenAdded = true;
            }

        }

    

       

       
         
        
            
        if(Is4Enemies)
        {
            if(Enemies[EnemieIndex].GetComponentsInChildren<EnemiesHealth>().Length < 2)
            {
                MissionSound.MissionSound_.i++;
                //MissionSound.MissionSound_.i = EnemieIndex;
                MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
                Is4Enemies = false;
            }
            else if (Enemies[EnemieIndex].GetComponentsInChildren<EnemiesHealth>().Length < 1)
            {
                MissionSound.MissionSound_.i++;
                //MissionSound.MissionSound_.i = EnemieIndex;
                MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
                Is4Enemies = false;
            }
            else if (Enemies[EnemieIndex].GetComponentsInChildren<EnemiesHealth>().Length < 0)
            {
                MissionSound.MissionSound_.i++;
                //MissionSound.MissionSound_.i = EnemieIndex;
                MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
                Is4Enemies = false;
            }



        }
        if (Enemies.Length < 1)
        {
            MissionSound.MissionSound_.Mission4(MissionWinPlay);
        }
       

    }
}
