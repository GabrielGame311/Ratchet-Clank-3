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
    private CinemachineFreeLook vCam; // Den faktiska kamerakomponenten

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
    public LayerMask groundLayer;

    public Camera Mycam;
    public CinemachineFreeLook Cinecam;

   

    // Lägg till dessa i toppen av ditt freefall-skript
    [Header("Landing Settings")]
    public float helicopterDistance = 5f; // Avståndet till marken när helikoptern startar
    public float helicopterFallSpeed = 5f; // Hur mycket han bromsar (lägre värde = långsammare)
    public bool isHelicoptering = false;

   

    private void Start()
    {

        Freefall = GetComponent<freefall>();
        controller = GetComponent<CharacterController>();
        
        ratchet = GetComponent<RatchetController>();
        anim = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();

        // Hämta Cinemachine-komponenten från ditt CamFreefall objekt
        if (CamFreefall != null)
            vCam = CamFreefall.GetComponent<CinemachineFreeLook>();
        vCam.enabled = false;
        CamFreefall.SetActive(true);
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
        // Aktivera freefall-kameran (den som tittar ner)
        CamFreefall.SetActive(true);
        vCam.enabled = true;

        // Hämta input
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");
        if (IOSController.IosController_ != null)
        {
            hInput = IOSController.IosController_.JoyStick_.Horizontal;
            vInput = IOSController.IosController_.JoyStick_.Vertical;
        }

        // Raycast för att se om vi ska fälla ut helikoptern
        RaycastHit hit;
        bool nearGround = Physics.Raycast(transform.position, Vector3.down, out hit, helicopterDistance, groundLayer);

        if (nearGround)
        {
            if (!isHelicoptering)
            {
                isHelicoptering = true;
                if (HelikopterController.Instance != null)
                    HelikopterController.Instance.StartHelikopter();

                anim.SetBool("FreeFall", false);
                
                anim.SetBool("IsHelicopter", true);
            }
            Glide();
        }
        else
        {
            // VANLIGT SNABBT FALL (Ratchet faller med huvudet först)
            isHelicoptering = false;
            anim.SetBool("FreeFall", true);

            // Rörelse i World Space (Gör att kameran INTE roterar)
            Vector3 moveDir = new Vector3(vInput, 0, -hInput).normalized;
            Vector3 fallVelocity = (Vector3.down * falldawnSpeed) + (moveDir * MoveSpeed);

            // Ändra alla rader där du har controller.Move till detta:
            if (controller != null && controller.enabled)
            {
                controller.Move(fallVelocity * Time.deltaTime);
            }
        }
    }

    public void Glide()
    {
        // 1. Rörelse i sidled (World Space)
        // Vi använder hInput/vInput direkt så vi inte behöver hämta dem igen
        Vector3 moveDir = new Vector3(hInput, 0, vInput).normalized;

        // 2. Hastigheter (R&C 3 värden)
        float glideDescentSpeed = 4f; // Saktar ner fallet ordentligt
        float glideSideSpeed = MoveSpeed * 0.7f; // Lite segare styrning i sidled

        // 3. Slå ihop neråt-fart och styrning
        Vector3 finalVelocity = (Vector3.down * glideDescentSpeed) + (moveDir * glideSideSpeed);

        // 4. Utför förflyttningen
        controller.Move(finalVelocity * Time.deltaTime);

        // VIKTIGT: Vi roterar INTE Ratchet här. 
        // Om du vill att han ska luta lite, rotera bara grafiken/modellen, inte hela spelar-objektet.
    }

    public void StopFalling()
    {
        // Lägg till i StopFalling
        if (vCam != null)
        {
            var noise = vCam.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null)
            {
                noise.m_AmplitudeGain = 2f; // Starta skak
                                            // Du kan använda en Invoke eller Coroutine för att stänga av skaket efter 0.2 sekunder
            }
        }

        Cinecam.enabled = false;
        isHelicoptering = false;
        vCam.enabled = false;
        CamFreefall.SetActive(false);
        ItsFalling = false;
        IsMoving = false;
        ratchet.Gravity = GravityStart;
        ratchet.enabled = true;
       // ratchet.CanMove = true;
        
       
        anim.SetBool("FreeFall", false);
    }
    // Hanterar vad som händer när vi INTE faller
    void HandleNormalState()
    {
        // Säkerställ att vi inte råkar ha kvar fall-inställningar
        if (ratchet != null)
        {
           // ratchet.enabled = true;
            //ratchet.CanMove = true;
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
