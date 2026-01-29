using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISight3D : MonoBehaviour
{

    public GameObject[] sight3D;
    public int Sight_ID;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
       
        


    }



    public void UseSight(int SightActive)
    {


        Sight_ID = SightActive;

        foreach (GameObject sight in sight3D)
        {
            if (sight != null) sight.SetActive(false);
        }

        // Activate the selected sight
        if (SightActive >= 0 && SightActive < sight3D.Length)
        {
            sight3D[SightActive].SetActive(true);
            Sight_ID = SightActive;
        }

    }
}
