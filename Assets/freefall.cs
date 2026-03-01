using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Cinemachine;



public class freefall : MonoBehaviour
{
    [Header("Components")]
    public CharacterController controller;
    private RatchetController ratchet; // Cacha denna för prestanda
    private Animator anim;

    [Header("Camera Settings")]
    public GameObject CamFreefall;
    private CinemachineVirtualCamera vCam; // Den faktiska kamerakomponenten

    [Header("Fall Settings")]
    public float falldawnSpeed = 20f;
    public float MoveSpeed = 8f;
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
        
        ratchet = GetComponent<RatchetController>();
        anim = GetComponent<Animator>();

        // Hämta Cinemachine-komponenten från ditt CamFreefall objekt
        if (CamFreefall != null)
            vCam = CamFreefall.GetComponent<CinemachineVirtualCamera>();

        GravityStart = ratchet.Gravity;

    }

    private void Update()
    {
        if (ItsFalling)
        {
            HandleFalling();
        }
        else
        {
            HandleNormalState();
        }

        HandleRootMotion();

        if (controller.isGrounded && ItsFalling)
        {
            StopFalling();
        }
    }

    void HandleFalling()
    {
        anim.SetBool("FreeFall", true);
        CamFreefall.SetActive(true);
        ratchet.CanMove = false;
        ratchet.enabled = false;

        // 1. Rörelse nedåt
        controller.Move(Vector3.down * falldawnSpeed * Time.deltaTime);

        // 2. Rörelse i sidled (Styrning)
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        if (IOSController.IosController_ != null)
        {
            hInput = IOSController.IosController_.JoyStick_.Horizontal;
            vInput = IOSController.IosController_.JoyStick_.Vertical;
        }

        pos = new Vector3(hInput, 0, vInput);
        controller.Move(pos * MoveSpeed * Time.deltaTime);

        // 3. Dynamisk FOV (Fartkänsla!)
        if (vCam != null)
        {
            // Vi baserar FOV på falldawnSpeed. Ju snabbare fall, desto mer FOV.
            // Här mappar vi hastigheten (t.ex. 20) till ett FOV mellan 60 och 90.
            float targetFOV = Mathf.Lerp(60, 90, falldawnSpeed / 50f);
            vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, targetFOV, Time.deltaTime * 2f);
        }

        // Ljudhantering
        if (PlayMusic && sound != null)
        {
            sound.gameObject.SetActive(true);
            MusicPlayed.gameObject.SetActive(false);
        }
    }

    void StopFalling()
    {
        ItsFalling = false;
        IsMoving = false;
        ratchet.Gravity = GravityStart;
        ratchet.enabled = true;
        ratchet.CanMove = true;
        CamFreefall.SetActive(false);
        anim.SetBool("FreeFall", false);
    }
    // Hanterar vad som händer när vi INTE faller
    void HandleNormalState()
    {
        // Säkerställ att vi inte råkar ha kvar fall-inställningar
        if (ratchet != null)
        {
            ratchet.enabled = true;
            ratchet.CanMove = true;
        }

        if (CamFreefall != null && CamFreefall.activeSelf)
        {
            CamFreefall.SetActive(false);
        }

        if (anim != null)
        {
            anim.SetBool("FreeFall", false);
        }

        // Stäng av vindljudet om det körs
        if (sound != null && sound.gameObject.activeSelf)
        {
            sound.gameObject.SetActive(false);
            MusicPlayed.gameObject.SetActive(true);
        }
    }

    // Hanterar Root Motion baserat på om det är en cutscene eller inte
    void HandleRootMotion()
    {
        if (IsScene)
        {
            // Om vi är i en specifik mission-scen (från ditt originalskript)
            if (EnemiesMission.instance != null && EnemiesMission.instance.Mission == 0)
            {
                anim.applyRootMotion = IsMoving;
            }
        }
        else
        {
            // Standardläge: Root Motion är på för normal rörelse
            anim.applyRootMotion = true;
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
