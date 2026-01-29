using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class LoadingMenu : MonoBehaviour
{

    public TMP_Text LoadProcentText;

    public Slider SliderLoading;

    public float LoadingTime = 5.0f;

    float startTime;

    public static LoadingMenu Instance;
    public string LoadScene;

    public int StartMap;
   


    // Start is called before the first frame update
    void Start()
    {

      

        Instance = this;
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {

        
            // Calculate the progress as a value between 0 and 1
            float progress = (Time.time - startTime) / LoadingTime;

            // Ensure the progress doesn't exceed 1
            progress = Mathf.Clamp01(progress);

            // Update the slider value based on the progress
            SliderLoading.value = progress;

            // Update the text to display the loading percentage
            LoadProcentText.text = Mathf.Round(progress * 100f) + "%";

            // Check if loading is complete
            if (progress >= 1.0f)
            {
                // Loading is complete, you can perform any necessary actions here
                Debug.Log("Loading complete!");

                LoadMapName.Instance.LoadMap = LoadScene;
                SceneManager.LoadScene("LoadingMap 1");

                LoadMapName.Instance.SpawnNewGame();
                



                //LoadingScene.LoadScene.LoadMap = LoadScene;
            }
       

       
    }

}

