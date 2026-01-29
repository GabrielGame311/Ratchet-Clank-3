using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;



public class IntroScene : MonoBehaviour
{

    [SerializeField] private VideoPlayer VideoPlayer_;
    [SerializeField] private VideoClip[] Video_;
    public int currentClipIndex = 0;

    void Start()
    {
        // Validate setup
        if (VideoPlayer_ == null)
        {
            Debug.LogError("VideoPlayer_ is not assigned!");
            return;
        }
        if (Video_ == null || Video_.Length == 0)
        {
            Debug.LogError("Video_ array is empty or not assigned!");
            return;
        }

        // Start with the first clip
        PlayClip(0);
    }


    private void Update()
    {

        if(currentClipIndex == 1)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

       
    }


    private void PlayClip(int index)
    {
        // If no more clips, load MainMenu
        if (index >= Video_.Length)
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        // Assign and play the current clip
        VideoPlayer_.clip = Video_[index];
        VideoPlayer_.isLooping = false;
        VideoPlayer_.loopPointReached += OnVideoEnded;
        VideoPlayer_.Play();
    }

    private void OnVideoEnded(VideoPlayer vp)
    {
        // Unsubscribe to avoid multiple triggers
        vp.loopPointReached -= OnVideoEnded;

        // Move to the next clip
        currentClipIndex++;

        // Play next clip or load MainMenu if done
        PlayClip(currentClipIndex);
    }
}
