using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Menu : MonoBehaviour
{

    public GameObject MainMenu;

    public static bool IsMenu = false;

    public Wrench sound;
    public Slider slider;
    
    public AudioMixer AudioMixer;

   

    public CinemachineFreeLook cine;
    


    private bool intialized = false;


    private void Start()
    {


        cine = GameObject.FindObjectOfType<CinemachineFreeLook>();

       if(PlayerPrefs.HasKey("volume"))
       {
            slider.value = PlayerPrefs.GetFloat("volume");
            AudioMixer.updateMode = (AudioMixerUpdateMode)PlayerPrefs.GetFloat("volume");
       }
        intialized = true;


        
       
    }



    void Update()
    {
       
        
            if (Input.GetKeyDown(KeyCode.Escape))
            {
               if(IsMenu)
               {
                Resume();

               }
               else
               {
                Pause();
               }
               

            }


        if(Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.DeleteAll();
        }
        
        
        
    }



    public void Resume()
    {
        Time.timeScale = 1;
       
        MainMenu.SetActive(false);
        cine.enabled = true;
        IsMenu = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Pause()
    {
        Time.timeScale = 0;
        
        MainMenu.SetActive(true);
        cine.enabled = false;
        IsMenu = true;
        Cursor.lockState = CursorLockMode.None;
    }




    public void SetVolume (float volume)
    {
        if (!intialized) return;
        if (!Application.isPlaying) return;
        AudioMixer.SetFloat("volume", volume);
        PlayerPrefs.SetFloat("volume", volume);
    }

   
    public void LoadMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }
    
 
}
