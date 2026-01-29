using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGadgetron : MonoBehaviour
{

    public float RotateSpeedPlus;
    public float RotateSpeedMinus;
   



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, +RotateSpeedPlus);
        transform.Rotate(0, 0, -RotateSpeedMinus);
    }
}
