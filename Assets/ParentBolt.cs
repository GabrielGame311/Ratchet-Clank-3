using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentBolt : MonoBehaviour
{



    public void getbolt()
    {
        GameObject.FindObjectOfType<BoltGet>().getbolt();


    }

    public void disableparent()
    {
        GameObject.FindObjectOfType<BoltGet>().disableparent();
    }
}
