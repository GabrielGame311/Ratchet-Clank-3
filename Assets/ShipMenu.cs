using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Playables;

public class ShipMenu : MonoBehaviour
{
    public GameObject Menu;
    public bool ItsTrigger = false;
    bool Pressed = false;
    float time = 3;
    public static ShipMenu ShipMenu_;
    bool ItsMenu = false;
    public GameObject Ship_Text;
    public string loadScene;



    private void Start()
    {
        ShipMenu_ = GetComponent<ShipMenu>();
       // ShipMenuTrigger.shipmenutrigger_.LandingTimeline.stopped += OnDirectorStopped2;
        GameObject.FindObjectOfType<ShipMenuTrigger>().LandingTimeline.stopped += OnDirectorStopped2;
    }

    private void Update()
    {
        if (ItsTrigger)
        {

            Ship_Text.SetActive(true);


            if (Input.GetKeyDown(KeyCode.E))
            {

                if(ItsMenu)
                {
                    Time.timeScale = 1f;
                    Menu.SetActive(false);
                    ShipMenuTrigger.shipmenutrigger_.StartShip.enabled = false;
                    
                    Cursor.lockState = CursorLockMode.Locked;
                    Player.Player_.GetComponentInChildren<CinemachineFreeLook>().enabled = true;
                    ItsMenu = false;
                    // ShipMenuTrigger.shipmenutrigger_.Music.gameObject.SetActive(true);
                    GameObject.FindObjectOfType<MusicPlay>().AudioSource_.Play();

                }
                else
                {
                    
                 
                    GameObject.FindObjectOfType<MusicPlay>().AudioSource_.Pause();
                   // ShipMenuTrigger.shipmenutrigger_.Music.gameObject.SetActive(false);
                    ShipMenuTrigger.shipmenutrigger_.StartShip.enabled = true;
                    ShipMenuTrigger.shipmenutrigger_.StartShip.Play();
                    ShipMenuTrigger.shipmenutrigger_.StartShip.stopped += OnDirectorStopped;
                    Cursor.lockState = CursorLockMode.None;
                    ShipMenuTrigger.shipmenutrigger_.player.SetActive(false);
                    Player.Player_.GetComponentInChildren<CinemachineFreeLook>().enabled = false;
                    
                    ItsMenu = true;
                    SaveSystem.SaveGame(AllGameData.Instance.CurrentSaveSlot);
                }



               


            }
           
        }
        else
        {
            Ship_Text.SetActive(false);
        }

       
       



    }



    public void BackButton()
    {
        Time.timeScale = 1;
        //ShipMenuTrigger.shipmenutrigger_.StartShip.enabled = false;

       
        ShipMenuTrigger.shipmenutrigger_.LandingTimeline.enabled = true;
        ShipMenuTrigger.shipmenutrigger_.LandingTimeline.Play();
        Cursor.lockState = CursorLockMode.Locked;
        Player.Player_.GetComponentInChildren<CinemachineFreeLook>().enabled = true;
        ShipMenuTrigger.shipmenutrigger_.player.SetActive(false);
        Menu.SetActive(false);
        ItsMenu = false;

    }
    private void OnDirectorStopped(PlayableDirector director)
    {
        // Check if the stopped PlayableDirector is the one we're interested in.
        if (director == ShipMenuTrigger.shipmenutrigger_.StartShip)
        {
            // Perform your action when the timeline playback is completed.
            Debug.Log("Director playback completed!");
            Menu.SetActive(true);
           
            // ShipMenuTrigger.shipmenutrigger_.StartShip.enabled = false;
            Time.timeScale = 0f;
            // You can add your custom logic here.
        }
    }


    private void OnDirectorStopped2(PlayableDirector director2)
    {
        // Check if the stopped PlayableDirector is the one we're interested in.
        if (director2 == ShipMenuTrigger.shipmenutrigger_.LandingTimeline)
        {
            // Perform your action when the timeline playback is completed.
            Debug.Log("Director playback completed!");
            ShipMenuTrigger.shipmenutrigger_.player.SetActive(true);
            ShipMenuTrigger.shipmenutrigger_.LandingTimeline.enabled = false;
            
            GameObject.FindObjectOfType<MusicPlay>().AudioSource_.Play();

            // You can add your custom logic here.
        }
    }
}
