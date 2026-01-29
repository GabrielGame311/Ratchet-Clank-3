using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class RangerShip : MonoBehaviour
{
    public GameObject RangerShipMenu_;
    public PlayableDirector playableDirector_;
    GameObject Player_;
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
            GameObject.FindObjectOfType<CinemachineFreeLook>().enabled = false;
            // Få alla MonoBehaviour-komponenter från Player och dess barn
            

            // Gå igenom alla hittade komponenter och sätt enabled till false
            foreach (MonoBehaviour script in allPlayerScripts)
            {
                script.enabled = false;
                
            }

            Debug.Log("Alla skript på spelaren inaktiverade!");
        }
        else
        {
            Debug.LogWarning("Player_ är inte tilldelad eller kunde inte hittas!");
        }
    }
    public void EnableAllScriptsOnPlayer()
    {
        if (Player_ != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1;
            RangerShipMenu_.SetActive(false);
            GameObject.FindObjectOfType<CinemachineFreeLook>().enabled = true;
            Player_.transform.position = SpawnPoint.transform.position;
            CurrentSong = 1;
            Music.clip = Songs[CurrentSong];
            Music.Play();
            // Få alla MonoBehaviour-komponenter från Player och dess barn

            // Gå igenom alla hittade komponenter och sätt enabled till false
            foreach (MonoBehaviour script in allPlayerScripts)
            {
                script.enabled = true;
                
            }

            Debug.Log("Alla skript på spelaren inaktiverade!");
        }
        else
        {
            Debug.LogWarning("Player_ är inte tilldelad eller kunde inte hittas!");
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
