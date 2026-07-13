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

    void Start()
    {
        MissionSound_ = this;
        sound = GetComponent<AudioSource>();

        if (PlayYes)
        {
            IsPlay = true;
        }
    }

    void Update()
    {
        if (IsPlays)
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
        // Säkerhetskontroll så att vi inte försöker spela utanför arrayens gränser
        if (Music == null || Music.Length == 0 || i >= Music.Length) return;

        sound.clip = Music[i];
        sound.Play();

        // =================================================================
        // NY SYNKRONISERING: KOPPLING TILL GALACTIC RANGERS
        // =================================================================
        // Letar upp GalacticRangers i scenen

        foreach(GalacticRangers ranger in FindObjectsOfType<GalacticRangers>())
        {
            if (ranger != null)
            {
                // Vi sätter robotens spline-index till att matcha musikens index (i)
                ranger.currentTargetSplineIndex = i;

                // Avbryt vänteläget så att roboten börjar springa på sin nya spline direkt!
                ranger.isWaitingForSignal = false;

                Debug.Log($"MissionSound: Matchar Rangers Spline Index till {i} och skickar startsignal!");
            }
        }

       
        
    }

    /// <summary>
    /// Anropa denna metod från andra skript (t.ex. EnemiesMission) när du vill byta låt/våg
    /// </summary>
    public void NextMissionWave()
    {
        i++; // Gå till nästa låtindex (1, 2, 3...)
        IsPlay = true;
        IsPlays = true;
    }
}