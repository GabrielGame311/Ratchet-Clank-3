using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class QuickSelect : MonoBehaviour
{
    
    public GameObject QuickSelects;
   
    private Animator anime;
    private static bool IsSelect;
    private GameObject wrench;


    PlayerControlls controlls;


    public CinemachineFreeLook cine;

    void Start()
    {
        QuickSelects.SetActive(false);


        cine = GameObject.FindObjectOfType<CinemachineFreeLook>();



    }


    private void OnEnable()
    {

       
        controlls.PlaystationControlls.Enable();

        
    }


    private void OnDisable()
    {
        
        controlls.PlaystationControlls.Disable();
    }



    



    // Update is called once per frame
    void Update()
    {
        
      
       
        

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (IsSelect)
            {
                Time.timeScale = 1;
                
                QuickSelects.SetActive(false);
                if (GameObject.FindObjectOfType<WeaponAmmos>() != null)
                {
                    GameObject.FindObjectOfType<WeaponAmmos>().enabled = true;
                }

                cine.enabled = true;
                IsSelect = false;
                GameObject.FindObjectOfType<ThirdPersonCamera>().enabled = true;
                if (!GameObject.FindObjectOfType<GadgeTronArmor>().isGadgeTron)
                {
                    
                    
                    Cursor.lockState = CursorLockMode.Locked;
                }
                
            }
            else
            {

                cine.enabled = false;
                if (GameObject.FindObjectOfType<WeaponAmmos>() != null)
                {
                    GameObject.FindObjectOfType<WeaponAmmos>().enabled = false;
                }
                Cursor.lockState = CursorLockMode.None;
                QuickSelects.SetActive(true);
                Time.timeScale = 0;
                IsSelect = true;
                GameObject.FindObjectOfType<ThirdPersonCamera>().enabled = false;


                if (!GameObject.FindObjectOfType<GadgeTronArmor>().isGadgeTron)
                {


                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            


            wrench.SetActive(true);
            
        }
        if (IsSelect == true)
        {
           


            

           
        }

    }


    
   
}
