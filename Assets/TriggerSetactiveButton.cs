using NUnit.Framework;
using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;


public class TriggerSetactiveButton : MonoBehaviour
{

    public KeyCode Button_;
    bool Istrigger = false;
    public List<GameObject> SetactiveObject;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Istrigger)
        {
            if(Input.GetKeyDown(Button_))
            {
                foreach(GameObject go in SetactiveObject)
                {
                    go.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Istrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Istrigger = false;
    }
}
