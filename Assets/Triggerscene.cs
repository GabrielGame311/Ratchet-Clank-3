using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class Triggerscene : MonoBehaviour
{
    public PlayableDirector playable;

    GameObject player;
    public GameObject Freefall;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            player = other.gameObject;
            playable.stopped += OnDirectorStopped;
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.parent = gameObject.transform.parent;
            playable.Play();
        }
    }
    private void OnDirectorStopped(PlayableDirector director)
    {
        // Check if the stopped PlayableDirector is the one we're interested in.
        if (director == playable)
        {
            player.GetComponent<CharacterController>().enabled = true;
            player.transform.parent = null;
            Freefall.SetActive(true);
            player.GetComponent<freefall>().ItsFalling = true;
            // Perform your action when the timeline playback is completed.
            Debug.Log("Director playback completed!");

            // You can add your custom logic here.
        }
    }



}
