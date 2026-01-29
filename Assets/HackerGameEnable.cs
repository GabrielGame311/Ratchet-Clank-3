using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackerGameEnable : MonoBehaviour
{

    public GameObject HackerGame;
    public GameObject InGame;
    GameObject player_;
    public HackerGameUI hackerGame;



    // Start is called before the first frame update
    void Start()
    {
        
        player_ = GameObject.FindGameObjectWithTag("Playerholder");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void EnableHackerGame()
    {
        player_.SetActive(false);
        InGame.SetActive(false);
        HackerGame.SetActive(true);
        hackerGame.gameObject.SetActive(true);
    }

    public void DisableHackerGame()
    {
        player_.SetActive(true);
        InGame.SetActive(true);
        HackerGame.SetActive(false);
        hackerGame.gameObject.SetActive(false);
    }

}
