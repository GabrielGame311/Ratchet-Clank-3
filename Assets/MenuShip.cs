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



    public void SceneLoad()
    {
        Time.timeScale = 1f;
        Button_.interactable = false;
        ShipMenu.ShipMenu_.Menu.SetActive(false);
        ShipMenuTrigger.shipmenutrigger_.Flytime.enabled = true;
        ShipMenuTrigger.shipmenutrigger_.Flytime.Play();
        ShipMenuTrigger.shipmenutrigger_.Flytime.stopped += OnDirectorStopped;

        ShipMenu.ShipMenu_.loadScene = SceneName;
        // LoadingScene.LoadScene.LoadMap = SceneName;



    }

    private void OnDirectorStopped(PlayableDirector director)
    {
        // Check if the stopped PlayableDirector is the one we're interested in.
        if (director == ShipMenuTrigger.shipmenutrigger_.Flytime)
        {
            // Perform your action when the timeline playback is completed.
            Debug.Log("Director playback completed!");

            LoadMapName.Instance.LoadMap = SceneName;
            LoadMapName.Instance.mapid = Scenes;
            ShipMenuTrigger.LoadShipScene();
             SceneManager.LoadScene("LoadingMap 1");
            
            // You can add your custom logic here.
        }
    }

}
