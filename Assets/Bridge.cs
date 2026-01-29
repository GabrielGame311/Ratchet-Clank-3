using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{


    private Movment movment;
    public GameObject timeline;
    public Transform player;
    public Transform spawn;
    private Collider collidier;
    private CharacterController controller;
    public Transform look;

    public bool hacker = false;

    public Animator ShootingAnime;
    public Animator ShootingAnimeScene;
    public Firing_aquatosEnemie Aquatos_Shootin;

    public bool DestroyObject = false;

    private void Start()
    {
        collidier = GetComponent<BoxCollider>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
        
    }

    private void Update()
    {



        if (hacker == true)
        {
            timeline.SetActive(true);
            StartCoroutine(wait());
        }





    }

    


    private void OnTriggerEnter(Collider other)
    {
       

        if (other.tag == "Player")
        {


            if (hacker == true)
            {
                timeline.SetActive(true);
                StartCoroutine(wait());
            }


            if (hacker == false)
            {



                ShootingAnime.SetBool("Walk", true);
                ShootingAnimeScene.SetTrigger("Scene1");
                Aquatos_Shootin.Isfiring = true;

                if(Aquatos_Shootin.Isfiring == true)
                {
                    ShootingAnime.SetBool("Walk", false);
                    
                }
            }

        }


       



        
    }

    public void Spawn()
    {
        controller.enabled = false;
        player.transform.position = spawn.transform.position;
        player.LookAt(spawn.transform);

        
       


    }

    IEnumerator wait()
    {

       
        

        yield return new WaitForSeconds(8.9f);

        controller.enabled = true;
        






        hacker = false;
        collidier.enabled = false;
    }



    
}
