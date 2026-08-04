using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class RangerShip : MonoBehaviour
{
    public GameObject RangerShipMenu_;
    public PlayableDirector playableDirector_;
    public GameObject Player_;
    public Transform SpawnPoint;
    MonoBehaviour[] allPlayerScripts;
    public AudioSource Music;
    public AudioClip[] Songs;
    public int CurrentSong;
    // Start is called before the first frame update
    void Start()
    {
        Music.clip = Songs[CurrentSong];
        Music.Play();
        Player_ = GameObject.FindGameObjectWithTag("Player");

         allPlayerScripts = Player_.GetComponentsInChildren<MonoBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisableAllScriptsOnPlayer()
    {
        if (Player_ != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
            RangerShipMenu_.SetActive(true);
             foreach (MonoBehaviour script in allPlayerScripts)
            {
                script.enabled = false;
                
            }
            Player_.GetComponent<CharacterController>().enabled = false;
            GameObject.FindObjectOfType<CinemachineFreeLook>().enabled = false;
            // F� alla MonoBehaviour-komponenter fr�n Player och dess barn
            

            // G� igenom alla hittade komponenter och s�tt enabled till false
           

            Debug.Log("Alla skript p� spelaren inaktiverade!");
        }
        else
        {
            Debug.LogWarning("Player_ �r inte tilldelad eller kunde inte hittas!");
        }
    }
    public void EnableAllScriptsOnPlayer()
    {
        if (Player_ != null)
        {
              foreach (MonoBehaviour script in allPlayerScripts)
            {
                script.enabled = true;
                
            }
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
            RangerShipMenu_.SetActive(false);
            Player_.transform.position = SpawnPoint.transform.position;
            GameObject.FindObjectOfType<CinemachineFreeLook>().enabled = true;
             Player_.GetComponent<CharacterController>().enabled = true;
            CurrentSong = 1;
            Music.clip = Songs[CurrentSong];
            Music.Play();
            // F� alla MonoBehaviour-komponenter fr�n Player och dess barn

            // G� igenom alla hittade komponenter och s�tt enabled till false
          

            Debug.Log("Alla skript p� spelaren inaktiverade!");
        }
        else
        {
            Debug.LogWarning("Player_ �r inte tilldelad eller kunde inte hittas!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            //playableDirector_.Play();
            DisableAllScriptsOnPlayer();


        }
    }
}
