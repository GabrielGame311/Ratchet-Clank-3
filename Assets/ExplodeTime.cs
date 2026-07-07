using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ExplodeTime : MonoBehaviour
{


    public float DieTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(die());
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator die()
    {
        yield return new WaitForSeconds(DieTime);

        Destroy(gameObject);
    }

    
}
