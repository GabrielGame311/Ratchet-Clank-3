using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponNefrouis : MonoBehaviour
{

    public GameObject Prefab_;
    public float Time_;
    float startTime;

    public Transform pos;

    public bool ISActive = false;
    public GameObject[] Weapons;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time_;
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject wp in Weapons)
        {
           


            if (ISActive)
            {
                wp.SetActive(true);
                Time_ -= Time.deltaTime;
                if (Time_ < 0)
                {

                    GameObject st = Instantiate(Prefab_, pos.transform.position, pos.transform.rotation);
                    Destroy(st, 2);
                    Time_ = startTime;


                }
            }
            else
            {
                wp.SetActive(false);
            }
        }



       

    }
}
