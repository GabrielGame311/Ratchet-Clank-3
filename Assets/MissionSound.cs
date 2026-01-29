using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionSound : MonoBehaviour
{

    public AudioSource sound;

    public AudioClip[] Music;
    public static MissionSound MissionSound_;
    float PlayTime = 3;
    public int i = 0;
    public bool IsPlay = false;

    public bool IsPlays = true;

    public bool PlayYes = true;

    
    // Start is called before the first frame update
    void Start()
    {
        MissionSound_ = GetComponent<MissionSound>();
        sound = GetComponent<AudioSource>();
        
        if(PlayYes)
        {
            IsPlay = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(IsPlays)
        {
            if (IsPlay)
            {

               

                    Mission4(i);

                    IsPlay = false;
                IsPlays = false;
            }
        }


       

       
       


    }


    public void Mission4(int iss)
    {
        iss = i;
        

       

        sound.clip = Music[i];
        sound.Play();


    }
}
