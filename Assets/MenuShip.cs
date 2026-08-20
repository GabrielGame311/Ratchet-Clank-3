using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MenuShip : MonoBehaviour
{

    public int Scenes;
    float time = 2;
    bool Pressed = false;
    public string SceneName;
    public static MenuShip menuship_;


    public Button Button_;

    // Start is called before the first frame update
    void Start()
    {
        menuship_ = GetComponent<MenuShip>();


        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == SceneName)
        {
            Button_.interactable = false;
        }
        else
        {
            Button_.interactable = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
       
        
       
    }


    public void SelectDestination()
    {
        // 1. Spara vilken bana skeppet ska åka till
       

        // 2. Sätt de statiska flaggorna så att LoadingScene förstår att det är en direkt resa
        LoadingScene.PendingLoadMap = SceneName;
        
        LoadingScene.HasPendingDirectMap = true;

        LoadMapName.DirectMapNavigation = true;
        LoadMapName.NextMapToLoad = SceneName;
      

        // 3. Stäng menyn och starta avfärds-Cutscenen
        Time.timeScale = 1f;
        

        // Spela cutscene för skeppet som flyger iväg
        if (ShipMenuTrigger.shipmenutrigger_ != null)
        {
            //ShipMenuTrigger.shipmenutrigger_.StartShip.enabled = true;
            //ShipMenuTrigger.shipmenutrigger_.StartShip.Play();
        }
    }

    public void SceneLoad()
    {
        Time.timeScale = 1f;

        // 1. Hämta den aktiva sparlådan
        int activeSlot = AllGameData.Instance != null ? AllGameData.Instance.CurrentSaveSlot : 0;

        // 2. Uppdatera AllGameData med den nya banans information
        if (AllGameData.Instance != null)
        {
            AllGameData.Instance.SavedMap = SceneName;
            AllGameData.Instance.CurrentMapInt = Scenes;
            AllGameData.Instance.hasCheckpoint = false; // Nollställ checkpoint inför landning
        }

        // 3. SPARA TILL save_X.json MED DEN NYA BANAN
        SaveSystem.SaveGame(activeSlot, SceneName, Scenes);

        // 4. Sätt statiska variabler för LoadingScene
        LoadingScene.PendingLoadMap = SceneName;
        LoadingScene.PendingMapId = Scenes;
        LoadingScene.HasPendingDirectMap = true;

        LoadMapName.SetDirectMap(SceneName, Scenes);
        LoadMapName.NextSaveSlot = activeSlot;

        // 5. Stäng UI och starta flyganimationen
        if (Button_ != null) Button_.interactable = false;

        if (ShipMenu.ShipMenu_ != null && ShipMenu.ShipMenu_.Menu != null)
        {
            ShipMenu.ShipMenu_.Menu.SetActive(false);
        }

        if (ShipMenuTrigger.shipmenutrigger_ != null && ShipMenuTrigger.shipmenutrigger_.Flytime != null)
        {
            ShipMenuTrigger.shipmenutrigger_.Flytime.stopped -= OnDirectorStopped;
            ShipMenuTrigger.shipmenutrigger_.Flytime.stopped += OnDirectorStopped;

            ShipMenuTrigger.shipmenutrigger_.Flytime.enabled = true;
            ShipMenuTrigger.shipmenutrigger_.Flytime.Play();
        }
    }

    private void OnDirectorStopped(PlayableDirector director)
    {
        if (ShipMenuTrigger.shipmenutrigger_ != null && 
            director == ShipMenuTrigger.shipmenutrigger_.Flytime)
        {
            director.stopped -= OnDirectorStopped;
            Debug.Log("Ship flight completed, loading LoadingMap 1...");

            LoadMapName.SetDirectMap(SceneName, Scenes);

            if (LoadMapName.Instance != null)
            {
                LoadMapName.Instance.LoadMap = SceneName;
                LoadMapName.Instance.mapid = Scenes;
            }

            ShipMenuTrigger.LoadShipScene();
            SceneManager.LoadScene("LoadingMap 1");
        }
    }

}
