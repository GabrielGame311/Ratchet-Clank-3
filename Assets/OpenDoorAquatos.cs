using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoorAquatos : MonoBehaviour
{

    public Animator anime;
    public Animator animebutton;
    public AudioSource sound;
    public GameObject cube;

    private void Start()
    {
        cube = GetComponent<GameObject>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            anime.SetTrigger("Open");
            animebutton.SetTrigger("Green");
            sound.Play();
           
            StartCoroutine(wait());


        }
        
        
        
    }


    IEnumerator wait()
    {
        yield return new WaitForSeconds(0.6f);


        if (sound.isPlaying)
        {
            sound.enabled = false;
        }
    }

}
