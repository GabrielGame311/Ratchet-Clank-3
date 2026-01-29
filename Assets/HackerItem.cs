using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackerItem : MonoBehaviour
{

    public Animator anime;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnEnable()
    {
        anime.SetBool("Gun", true);
    }

    private void OnDisable()
    {
        anime.SetBool("Gun", false);
    }
}
