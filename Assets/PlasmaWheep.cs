using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaWheep : MonoBehaviour
{

    private Animator anime;
    
    
    // Start is called before the first frame update
    void Start()
    {
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            anime.SetTrigger("Swing");
        }
        else
        {
            anime.SetTrigger("SwingAgain");
        }
    }
}
