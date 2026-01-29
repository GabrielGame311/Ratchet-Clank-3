using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class QuarkInfoUI : MonoBehaviour
{
    public TMP_Text Info_Text;
    public GameObject InfoPanel;
    public GameObject[] GamePanel;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject pl in GamePanel)
        {
            if (InfoPanel.activeSelf)
            {
                pl.SetActive(false);
            }
            else
            {
                pl.SetActive(true);
            }
        }
    }


    public void SetOnPanel(bool ISPanel)
    {
        if(ISPanel)
        {
            InfoPanel.SetActive(true);
           
        }
        else
        {
            InfoPanel.SetActive(false);
        }
    }


    public void OnText(string txt)
    {
        Info_Text.text = txt;
    }
}
