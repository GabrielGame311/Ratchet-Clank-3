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
    TempCheckpoint.Reset();
    if (render != null) render.gameObject.SetActive(true);
    if (Phinix != null) Phinix.SetActive(false);

    if (playable != null) playable.stopped += OnDirectorStopped;

    // PRIORITERING: Om vi åker med skeppet, tvinga den nya banan
    if (HasPendingDirectMap && !string.IsNullOrEmpty(PendingLoadMap))
    {
        LoadMap = PendingLoadMap;
        MapID = PendingMapId;
        Debug.Log("Direktresa godkänd till: " + LoadMap);
    }
    else if (LoadMapName.LoadingExistingSave && SaveUtility.TryReadSave(SaveSlot, out SaveData selectedSave))
    {
        LoadMap = string.IsNullOrEmpty(selectedSave.SavedMap) ? "Veldins" : selectedSave.SavedMap;
        MapID = selectedSave.CurrentMap;
    }
    else
    {
        LoadMap = LoadMapName.NextMapToLoad;
        MapID = LoadMapName.NextMapId;
    }

    // Uppdatera UI och material
    if (MapName_text != null) MapName_text.text = LoadMap;

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
            Debug.Log("Director playback completed! Loading: " + LoadMap);

            if (LoadMapName.Instance != null)
            {
                SaveSlot = LoadMapName.Instance.saveSlot;
            }

            // VIGTIGT: Återställ flaggorna så att sparfiler laddas korrekt nästa gång!
            HasPendingDirectMap = false;
            PendingLoadMap = "";
            LoadMapName.DirectMapNavigation = false;
            LoadMapName.LoadingExistingSave = false;

            SceneManager.LoadScene(LoadMap);
        }
    }

}
