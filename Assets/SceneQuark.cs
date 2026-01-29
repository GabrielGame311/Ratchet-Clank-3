using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class SceneQuark : MonoBehaviour
{
    public GameObject Scene;
    public VideoPlayer[] VideoPlayers;
    private int VideoPlayerInt = 0;


    public AudioSource[] Music;


    void Start()
    {
        Music = GameObject.FindObjectsOfType<AudioSource>();
        // Koppla eventet till alla VideoPlayers
        foreach (VideoPlayer vp in VideoPlayers)
        {
            vp.loopPointReached += OnVideoFinished;
        }

        // Starta första videon om det finns några i listan
        if (VideoPlayers.Length > 0)
        {
            PlayVideo(VideoPlayerInt);
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Öka indexet för att spela nästa video
        VideoPlayerInt++;

        if (VideoPlayerInt < VideoPlayers.Length)
        {
            PlayVideo(VideoPlayerInt);
        }
        else
        {
            // Om alla videor har spelats, inaktivera scenen
            foreach (AudioSource sound in Music)
            {
                sound.Play();
            }
            Scene.SetActive(false);
        }
    }

    void PlayVideo(int index)
    {
        // Se till att ingen annan video spelas
        foreach (VideoPlayer vp in VideoPlayers)
        {
            vp.Stop();
        }

        // Spela upp rätt video
        foreach(AudioSource sound in Music)
        {
            sound.Stop();
        }
        VideoPlayers[index].Play();
    }
}
