using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{

    public GameObject Wrench;
    public GameObject Torrets;
    public GameObject UITorret;

    public GameObject shotgun;
    public GameObject granadeshooter;
    public Animator anime;
    public GameObject cam_ThiredPerson;
    public GameObject Cam_FirstPerson;
    public GameObject Shotgun_sikte;

    public Shoutgun shotguns;
    public GranadeShooter granadeshooters;
    public GunBall gunballs;

    public Wrench wrench;
    // Start is called before the first frame update
    void Start()
    {
        
    }


  

    // Update is called once per frame
    void Update()
    {
       
        //Wrench Settings
        if(wrench.gameObject.activeSelf == false)
        {
            wrench.enabled = false;
           
            wrench.particleSplash.gameObject.SetActive(false);
        }
        if (wrench.gameObject.activeSelf == true)
        {
           

            wrench.enabled = true;
                wrench.particleSplash.gameObject.SetActive(true);
           


        }
        //Item:

        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (shotgun.activeSelf == true)
            {

                cam_ThiredPerson.SetActive(false);
                Cam_FirstPerson.SetActive(true);
                Shotgun_sikte.SetActive(true);
            }
            
            
        }
        else
        {
            cam_ThiredPerson.SetActive(true);
            Cam_FirstPerson.SetActive(false);
            Shotgun_sikte.SetActive(false);
        }

        //Granade 


       

        //Gun
        if (shotgun.activeSelf == true)
        {
            
        }
        else if(shotgun.activeSelf == false)
        {
            
        }
        

        if (Torrets.activeSelf == true)
        {
            
        }
        else if(Torrets.activeSelf == false)
        {
            
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            anime.SetBool("Gun", false);
            UITorret.SetActive(false);
            Torrets.SetActive(false);
            Wrench.SetActive(true);
            UITorret.SetActive(false);
            shotgun.SetActive(false);
            granadeshooter.SetActive(false);
            GameObject.FindObjectOfType<ParticleShootgun>().enabled = false;
            GameObject.FindObjectOfType<WrenchEnemies>().enabled = true;
           
        }


        //Torett

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            anime.SetBool("Gun", false);
            Torrets.SetActive(true);
            Wrench.SetActive(false);
            UITorret.SetActive(true);
            shotgun.SetActive(false);
            granadeshooter.SetActive(false);
        }
       

        //GUN1
        
        if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            shotgun.SetActive(true);
            UITorret.SetActive(false);
            Torrets.SetActive(false);
            Wrench.SetActive(false);
            UITorret.SetActive(false);
            anime.SetBool("Gun", true);
            granadeshooter.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            anime.SetBool("Gun", true);
            shotgun.SetActive(false);
            granadeshooter.SetActive(true);
            UITorret.SetActive(true);
            Torrets.SetActive(false);
            Wrench.SetActive(false);
           
        }


        if(granadeshooters.gameObject.activeSelf == false)
        {
            granadeshooters.shoot = true;
        }
        if (shotguns.gameObject.activeSelf == false)
        {
            shotguns.isfiring = true;
        }
        if (gunballs.gameObject.activeSelf == false)
        {
            gunballs.shoot = true;
        }

    }

    
}
