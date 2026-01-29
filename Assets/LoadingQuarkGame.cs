using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingQuarkGame : MonoBehaviour
{

    public GameObject LoadCircle;
    public float LoadTime;

    public GameObject LoadScreen;
    public GameObject GameStart;
    public float LoadCircleSpeed;



    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(0 < LoadTime)
        {
            LoadCircle.transform.Rotate(0, 0, +LoadCircleSpeed);
            LoadTime -= Time.deltaTime;
        }

        if(LoadTime < 0)
        {

            Loaded();
        }

    }



    private void Loaded()
    {

        LoadScreen.SetActive(false);
        GameStart.SetActive(true);

        
    }

}
