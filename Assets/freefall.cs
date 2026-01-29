using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class freefall : MonoBehaviour
{

    public CharacterController controller;
    Vector3 falldown;
    public float MovingSpeed;
    public GameObject CamFreefall;
    public float falldawnSpeed;
    public float MoveSpeed = 8;
    public bool ItsFalling = false;
    public static freefall Freefall;
    public bool IsMoving = false;
    public bool IsScene = false;
    public AudioSource sound;
    public AudioSource MusicPlayed;
    public Vector3 pos;
    public bool PlayMusic;
    public float hInput;
    public float vInput;
    float GravityStart;
    private void Start()
    {

        Freefall = GetComponent<freefall>();
        controller = GetComponent<CharacterController>();
        GravityStart = GetComponent<RatchetController>().Gravity;

    }

    private void Update()
    {

        

        if(ItsFalling)
        {
           
           // IsMoving = true;
            GetComponent<RatchetController>().anime.SetBool("FreeFall", true);
            CamFreefall.SetActive(true);
           
           // Camera.main.gameObject.SetActive(false);
            GetComponent<RatchetController>().CanMove = false;
            controller.Move(Vector3.down * falldawnSpeed * Time.deltaTime);


             hInput = Input.GetAxis("Horizontal");
             vInput = Input.GetAxis("Vertical");
            if (IOSController.IosController_ != null)
            {

                hInput = IOSController.IosController_.JoyStick_.Horizontal;
                vInput = IOSController.IosController_.JoyStick_.Vertical;
            }
          
            if(PlayMusic)
            {
                sound.gameObject.SetActive(true);
                MusicPlayed.gameObject.SetActive(false);
            }



           pos = new Vector3(hInput, 0, vInput);

            // Move the player based on input
            controller.Move(pos * MoveSpeed * Time.deltaTime);
            RatchetController.RatchetController_.enabled = false;


        }
        else
        {
            RatchetController.RatchetController_.enabled = true;
            GetComponent<RatchetController>().CanMove = true;
            CamFreefall.SetActive(false);
            GetComponent<RatchetController>().anime.SetBool("FreeFall", false);
            
            //Camera.main.gameObject.SetActive(true);
        }
      

        if(IsScene)
        {
            if(EnemiesMission.EnemiesMission_.Mission == 0)
            {
                if (IsMoving)
                {
                    GetComponent<Animator>().applyRootMotion = true;
                }
                else
                {
                    GetComponent<Animator>().applyRootMotion = false;
                }
            }
               
            
        }
        else
        {
            GetComponent<Animator>().applyRootMotion = true;
        }

       

        if(controller.isGrounded)
        {
            ItsFalling = false;
            IsMoving = false;
            GetComponent<RatchetController>().Gravity = GravityStart;
            
        }
    }


    public void RunForward()
    {
        sound.Play();
        IsMoving = true;


    }
    public void RunFalse()
    {

        IsMoving = false;


    }


}
