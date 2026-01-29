using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorUI : MonoBehaviour
{

    public GameObject ArmorUI_;

    public static ArmorUI instance;



    // Start is called before the first frame update
    void Start()
    {
        instance = GetComponent<ArmorUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ActiveArmorUI()
    {
        ArmorUI_.SetActive(true);
    }

    public void DisableArmorUI()
    {
        ArmorUI_.SetActive(false);
    }

}
