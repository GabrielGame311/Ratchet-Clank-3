using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;


public class TriggerMap : MonoBehaviour
{
    public PlayableDirector Flytime;
    public string LoadMap;
    public bool IsPlay;
    public static TriggerMap instance;
    public GameObject Camera;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
       
    }

    // Update is called once per frame
    void Update()
    {
        if(IsPlay)
        {

            IsPlay = false;
            Flytime.enabled = true;

            Flytime.Play();
        }
    }

    private void OnDirectorStopped(PlayableDirector director)
    {
        // Check if the stopped PlayableDirector is the one we're interested in.
        if (director == Flytime)
        {
            // Perform your action when the timeline playback is completed.
            Debug.Log("Director playback completed!");

            LoadMapName.Instance.LoadMap = LoadMap;
            SceneManager.LoadScene("LoadingMap 1");
           // LoadingScene.LoadScene.LoadMap = LoadMap;
            // You can add your custom logic here.
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Flytime.enabled = true;
            Flytime.stopped += OnDirectorStopped;
            ShipMenuTrigger.shipmenutrigger_.gameObject.SetActive(false);

            Camera.SetActive(true);
            Flytime.Play();
            other.gameObject.SetActive(false);
            GameObject.FindGameObjectWithTag("Playerholder").SetActive(false);

        }
    }


}
