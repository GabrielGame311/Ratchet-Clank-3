using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using TMPro;
using System.IO;
using System;

public class LoadingScene : MonoBehaviour
{

    public static LoadingScene LoadScene;
    public PlayableDirector playable;
    public string LoadMap;

    public TMP_Text MapName_text;
    public Renderer render;
    public Material[] Materials;
    public int MapID;
    public GameObject Phinix;
    public int SaveSlot;
    public static LoadingScene instance;

    // Start is called before the first frame update
    void Start()
    {

        instance = this;
        
        LoadScene = GetComponent<LoadingScene>();

        render.gameObject.SetActive(true);
        Phinix.SetActive(false);



        playable.stopped += OnDirectorStopped;

       
        


        LoadMap = LoadMapName.Instance.LoadMap;

        SaveSystem.SaveGame(SaveSlot);
        SaveUI saveUI = GameObject.FindObjectOfType<SaveUI>();
        if (saveUI != null)
        {
            saveUI.ShowSavingMessage();
        }


        MapName_text.text = LoadMap;

        MapID = LoadMapName.Instance.mapid;

        render.material = Materials[MapID];






       


    }

   



    private void Update()
    {
        if (LoadMap == "Phinix")
        {
            render.gameObject.SetActive(false);
            Phinix.SetActive(true);
        }



        if (LoadMap == "PhinixAttack")
        {
            Phinix.SetActive(true);
            render.gameObject.SetActive(false);

        }
    }


    private void OnDirectorStopped(PlayableDirector director)
    {
        // Check if the stopped PlayableDirector is the one we're interested in.
        if (director == playable)
        {
            // Perform your action when the timeline playback is completed.
            Debug.Log("Director playback completed!");
            SaveSystem.SaveGame(SaveSlot);
            

            SceneManager.LoadScene(LoadMap);
            // You can add your custom logic here.
        }
    }

}
