using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeadTrigger : MonoBehaviour
{
    public Animator animefade;
    public float CountdownKill;
    public Animator anime;
    public GameObject CamMovment;
    public GameObject CamDead;
    public AudioSource deadsound;

    private void Start()
    {
        anime.tag = "Player";
        animefade = GameObject.Find("fade").GetComponent<Animator>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            StartCoroutine(killed());
        }
    }

    IEnumerator killed()
    {
        deadsound.Play();
        CamMovment.SetActive(false);
        CamDead.SetActive(true);
        anime.SetTrigger("Fall");
        
        yield return new WaitForSeconds(CountdownKill);
        animefade.SetBool("Fade", true);

        yield return new WaitForSeconds(1.2f);
        SceneManager.LoadScene("Veldin");
    }


}
