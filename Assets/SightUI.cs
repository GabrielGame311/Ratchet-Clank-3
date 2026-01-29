using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightUI : MonoBehaviour
{

    public static SightUI SightUI_;
    public GameObject Sight;


    // Start is called before the first frame update
    void Start()
    {
        SightUI_ = GetComponent<SightUI>();
       // Sight = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
