using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject Menu;
    public GameObject VideoPlayer;
    public GameObject NotactiveVideo_;
    public float FloatTime;
    float StartTime;
    public string Map;
    public GameObject MainMenu_;

    private void Start()
    {
        StartTime = FloatTime;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            VideoPlayer.SetActive(false);
            Menu.SetActive(true);
        }

        if (NotactiveVideo_.activeSelf == false)
        {
            FloatTime -= Time.deltaTime;

            if (FloatTime < 0)
            {

                NotactiveVideo_.SetActive(true);
                MainMenu_.SetActive(false);

                FloatTime = StartTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            FloatTime = StartTime;
            NotactiveVideo_.SetActive(false);
            MainMenu_.SetActive(true);
        }

      


      


    }


    public void preesed()
    {
        VideoPlayer.SetActive(false);
        Menu.SetActive(true);
    }




    public void LoadVeldin()
    {
        SceneManager.LoadScene(Map);
   
    }

    public void LoadAqouatos()
    {
        SceneManager.LoadScene("LoadingMap");
    }


    public void LoadMap(string st)
    {
        SceneManager.LoadScene(st);
    }
}
