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
    public static string PendingLoadMap = "";
    public static int PendingMapId;
    public static bool HasPendingDirectMap;
    public PlayableDirector playable;
    public string LoadMap;

    public TMP_Text MapName_text;
    public Renderer render;
    public Material[] Materials;
    public int MapID;
    public GameObject Phinix;
    public int SaveSlot;
    public static LoadingScene instance;
    private bool destinationLoaded;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        LoadScene = GetComponent<LoadingScene>();

        render.gameObject.SetActive(true);
        Phinix.SetActive(false);

        playable.stopped += OnDirectorStopped;

        // Direct ShipMenu navigation always wins and never reads save JSON.
        SaveSlot = LoadMapName.NextSaveSlot;
        if (HasPendingDirectMap && !string.IsNullOrEmpty(PendingLoadMap))
        {
            LoadMap = PendingLoadMap;
            MapID = PendingMapId;
            Debug.Log("Direct ShipMenu map selected: " + LoadMap);
        }
        else if (LoadMapName.DirectMapNavigation && !string.IsNullOrEmpty(LoadMapName.NextMapToLoad))
        {
            LoadMap = LoadMapName.NextMapToLoad;
            MapID = LoadMapName.NextMapId;
        }
        else if (LoadMapName.LoadingExistingSave && SaveUtility.TryReadSave(SaveSlot, out SaveData selectedSave))
        {
            LoadMap = string.IsNullOrEmpty(selectedSave.SavedMap) ? "Veldins" : selectedSave.SavedMap;
            MapID = selectedSave.CurrentMap;
        }
        else
        {
            LoadMap = LoadMapName.NextMapToLoad;
        }
      

        // Säkerhet: Om strängen ändå skulle vara tom, ge en varning i konsolen
        if (string.IsNullOrEmpty(LoadMap))
        {
            Debug.LogError("LoadMap är tom! Kontrollera att du sätter LoadMapName.NextMapToLoad innan scenbyte.");
        }

        if (LoadMapName.Instance != null)
        {
            LoadMapName.Instance.saveSlot = SaveSlot;
            LoadMapName.Instance.LoadMap = LoadMap;
            LoadMapName.Instance.mapid = MapID;
        }

        if (MapName_text != null)
        {
            MapName_text.text = LoadMap;
        }

        if (Materials != null && MapID >= 0 && MapID < Materials.Length && render != null)
        {
            render.material = Materials[MapID];
        }
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
        if (director == playable && !destinationLoaded && !string.IsNullOrEmpty(LoadMap))
        {
            destinationLoaded = true;
            playable.stopped -= OnDirectorStopped;
            Debug.Log("Director playback completed!");

            // Sätt rätt SaveSlot från LoadMapName om den finns
            if (LoadMapName.Instance != null)
            {
                SaveSlot = LoadMapName.Instance.saveSlot;
            }

            SceneManager.LoadScene(LoadMap);
        }
    }

}
