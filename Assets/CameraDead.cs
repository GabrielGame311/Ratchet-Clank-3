using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDead : MonoBehaviour
{

    public Camera cam;
    public Transform player;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cam.transform.LookAt(player);
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
