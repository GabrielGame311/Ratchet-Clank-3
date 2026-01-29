using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCam : MonoBehaviour
{
    public GameObject FreelookCam;
    public GameObject MainCam;
    public GameObject FirstPersonCamera;

    public float mouseSensitivity = 100f;
    public Transform playerBody; // Dra in spelarens transform här i Unity Inspector

    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector3 initialPosition; // För att låsa positionen

    void Start()
    {
        
       
    }

    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            FreelookCam.SetActive(false);
            MainCam.SetActive(false);
            FirstPersonCamera.SetActive(true);
            UpdateFirstPersonCamera();
        }
        else
        {
            FreelookCam.SetActive(true);
            MainCam.SetActive(true);
            FirstPersonCamera.SetActive(false);
        }

        // Lås positionen om ingen annan rörelse är avsedd
        
    }

    void UpdateFirstPersonCamera()
    {
        // Hämta musinput för rotation
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Ackumulera rotation
        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // Applicera rotation på hela spelaren
        if (playerBody != null)
        {
            Quaternion targetRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            playerBody.rotation = targetRotation;
        }
    }

    
}