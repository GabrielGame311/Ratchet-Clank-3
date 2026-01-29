using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrowMission : MonoBehaviour
{

    public Camera cam;
    public bool IsTrigger = false;
    public Transform ScrowPoint;
    public Transform screw;
    public Transform LookThis;// Sätt skruvens transform här i Unity-inspektorn
    public float rotationSpeed = 50f; // Hastigheten som spelaren ska röra sig runt skruven
    private bool isInteracting = false;
    private Transform Player;
    bool Interact = false;
    Animator anime;
    RatchetController Controller;
    public bool IsScrowed = false;
    public Transform Wrench;
    public Transform WrenchPos;

    // Start is called before the first frame update
   
    void Start()
    {
        Wrench = GameObject.FindObjectOfType<WeaponSwitcher>().Wrench_;
        Controller = GameObject.FindGameObjectWithTag("Player").GetComponent<RatchetController>();
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();

        //GameObject.FindObjectOfType<SpawnTime>().RangerTalk();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsTrigger)
        {
            
            
            
                if (Input.GetKeyDown(KeyCode.E))
                {
                   
                    if(Interact)
                    {
                        GameObject.FindObjectOfType<WeaponSwitcher>().WrenchEnable();
                        Player.GetComponent<CharacterController>().enabled = true;
                        anime.SetBool("Scrow", false);
                        anime.SetBool("ScrowRun", false);
                        //Controller.enabled = true;
                        cam.gameObject.SetActive(false);

                        isInteracting = false;


                        Interact = false;
                    }
                    else
                    {


                        
                        // Börja interaktion om spelaren är nära skruven
                        cam.gameObject.SetActive(true);
                        //Controller.enabled = false;
                        anime.SetBool("Scrow", true);
                        GameObject.FindObjectOfType<WeaponSwitcher>().WrenchEnable();
                        Player.GetComponent<CharacterController>().enabled = false;
                            Interact = true;
                        float distance = Vector3.Distance(Player.position, screw.position);
                        if (distance < 2f)  // Justera avståndsgräns efter behov
                        {
                            isInteracting = !isInteracting; // Växla interaktionsläge
                        }
                    }
                }
            


            


            if (isInteracting)
            {

               // Wrench.LookAt(WrenchPos);
                
                //Wrench.localScale = WrenchPos.localScale;
                RotateAroundScrew();

                

            }
        }
    }

    void RotateAroundScrew()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            // Beräkna en rotationsrörelse runt skruvens position
            
            //Wrench.position = WrenchPos.position;
            //Wrench.rotation = WrenchPos.rotation;
            
            screw.Rotate(0, 0, 5+ Time.deltaTime);
            screw.transform.position -= Vector3.up * 0.1f * Time.deltaTime;
            Player.RotateAround(screw.position, Vector3.up, rotationSpeed * Time.deltaTime);
            Player.LookAt(LookThis);


            //anime.SetBool("Forward", true);



            anime.SetBool("ScrowRun", true);
        }
        else
        {
            anime.SetBool("ScrowRun", false);
        }

        if (Vector3.Distance(screw.position, ScrowPoint.position) < 0.01f)
        {
            // Stanna skruven exakt på målet och avsluta interaktionen
            screw.position = ScrowPoint.position;
            Player.GetComponent<CharacterController>().enabled = true;
            anime.SetBool("Scrow", false);
            anime.SetBool("ScrowRun", false);
            //Controller.enabled = true;
            cam.gameObject.SetActive(false);
            IsTrigger = false;
            isInteracting = false;
            GameObject.FindObjectOfType<SpawnTime>().RangerTalk();
            enabled = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            IsTrigger = true;
            Player = other.transform;
            


        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {


            IsTrigger = false;

            Player = null;
        }
    }
}
