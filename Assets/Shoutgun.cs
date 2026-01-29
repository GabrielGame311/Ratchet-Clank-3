using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class Shoutgun : MonoBehaviour
{

    public ParticleSystem particle;
    private Animator anime;
   
    public float NextFireTime;
    
    public bool isfiring = true;
    public Transform spawn;
    public bool IsAmmo = false;

    public InputAction fire;
     PlayerControlls controls;

    private void Start()
    {


        controls = new PlayerControlls();
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
       
        

        Debug.Log("soutgun");
    }



    private void OnEnable()
    {
       
        
        controls.Enable();
        fire = controls.PlaystationControlls.Shoot;

        fire.Enable();
        fire.performed += Fire;

    }

    private void OnDisable()
    {
        fire.Disable();
    }


    void Update()
    {
        if(GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo > 0)
        {

            
            IsAmmo = true;
        }
        else
        {
            IsAmmo = false;
        }

       



        if (IsAmmo == true)
        {

            if(controls.PlaystationControlls.Shoot.IsPressed())
            {
                if (isfiring == true)
                {
                    GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo -= 1;
                    anime.SetTrigger("Shoot");

                    particle.Play();
                    StartCoroutine(shoot());
                }

            }

            if(GetComponent<WeaponAmmos>().enabled)
            {
                if (IOSController.IosController_ == null)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        Shoots();
                    }
                }
            }

            






        }











    }

    public void Shoots()
    {
        if(IsAmmo == true)

        if (isfiring == true)
        {
            GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo -= 1;
            anime.SetTrigger("Shoot");
                if (GetComponent<WeaponAmmos>().Ammo < GetComponent<WeaponAmmos>().MaxAmmo)
                {
                    GameObject.FindObjectOfType<VendingMenu>().Price += GetComponent<WeaponAmmos>().havePrice;
                }
                particle.Play();
            StartCoroutine(shoot());
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
       


       
        
            if (isfiring == true)
            {
                Debug.Log("We fired");
                GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo -= 1;
                anime.SetTrigger("Shoot");

                particle.Play();
                StartCoroutine(shoot());
            }
        

    }
    IEnumerator shoot()
    {

        particle.Play();
        isfiring = false;

       

        yield return new WaitForSeconds(1f);
      
        isfiring = true;
      


    }


   

}
