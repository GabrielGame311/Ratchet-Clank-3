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
        // S�kerhetskontroll s� att vi inte f�rs�ker spela utanf�r arrayens gr�nser
        if (Music == null || Music.Length == 0 || i >= Music.Length) return;
        i = iss;
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
                // Vi s�tter robotens spline-index till att matcha musikens index (i)
                ranger.currentTargetSplineIndex = i;

                // Avbryt v�ntel�get s� att roboten b�rjar springa p� sin nya spline direkt!
                ranger.isWaitingForSignal = false;

                Debug.Log($"MissionSound: Matchar Rangers Spline Index till {i} och skickar startsignal!");
            }
        }

       
        
    }

    /// <summary>
    /// Anropa denna metod fr�n andra skript (t.ex. EnemiesMission) n�r du vill byta l�t/v�g
    /// </summary>
    public void NextMissionWave()
    {
        i++; // G� till n�sta l�tindex (1, 2, 3...)
        IsPlay = true;
        IsPlays = true;
    }
}