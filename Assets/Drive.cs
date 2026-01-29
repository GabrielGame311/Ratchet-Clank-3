using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive : MonoBehaviour
{
    public Animator anime;
    public GameObject camera;
    public GameObject cube;
    public GameObject player;
    
    public int speed;
    public GameObject timelinescene;
    public Animator fade;
    public Camera maincam;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        fade = GameObject.Find("fade").GetComponent<Animator>();
        maincam = Camera.main;
        camera.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            
            anime.SetTrigger("His");
            player.transform.SetParent(cube.transform);
            fade.SetBool("Fade", true);
            camera.SetActive(true);
            maincam.enabled = false;
            timelinescene.SetActive(true);
            StartCoroutine(wait());

        }
    }


    IEnumerator wait()
    {
        yield return new WaitForSeconds(6);
        
        fade.SetBool("Fade", false);
        timelinescene.SetActive(false);
        camera.SetActive(false);
        maincam.enabled = true;
        StartCoroutine(wait2());
        
    }

    IEnumerator wait2()
    {
        yield return new WaitForSeconds(3);
        player.transform.SetParent(null);
        
    }
}
