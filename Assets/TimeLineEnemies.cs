using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLineEnemies : MonoBehaviour
{

    public GameObject[] Enemies;
    public GameObject Timeline;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Enemies.Length < 1)
        {
            Timeline.SetActive(true);
        }
        else
        {
            Timeline.SetActive(false);
        }
        
    }
}
