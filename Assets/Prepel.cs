using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prepel : MonoBehaviour
{
    public float Speed = 5;



    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, +Speed);

    }
}
