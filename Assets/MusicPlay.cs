using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlay : MonoBehaviour
{
    public AudioSource AudioSource_; // Reference to the AudioSource component
    public AudioClip[] AudioClips_; // Array of AudioClips
    public int AudioPlay; // Integer to select which clip to play

    private int currentAudioPlay = -1; // Tracks the last played AudioPlay value

    // Start is called before the first frame update
    void Start()
    {
        if (AudioClips_ == null || AudioClips_.Length == 0)
        {
            Debug.LogError("No audio clips assigned!");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if AudioPlay has changed and is within a valid range
        if (AudioPlay != currentAudioPlay && AudioPlay >= 0 && AudioPlay < AudioClips_.Length)
        {
            currentAudioPlay = AudioPlay; // Update the current playing track index

            // Assign the new clip to the AudioSource
            AudioSource_.clip = AudioClips_[AudioPlay];

            // Play the new clip
            AudioSource_.Play();
        }
    }

    public void ChangeSound()
    {
        AudioPlay++;
    }

    public void ChangeSoundBack()
    {
        AudioPlay--;
    }
}
