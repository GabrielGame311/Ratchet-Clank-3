using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    public GameObject wrench;
    public Transform WrenchPos;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



   
    public void Throws()
    {
        //wrench.transform.parent = null;
        GameObject.FindObjectOfType<Wrench>().Throw1();
        wrench.transform.position = WrenchPos.transform.position;
        wrench.transform.rotation = WrenchPos.transform.rotation;

    }
}
