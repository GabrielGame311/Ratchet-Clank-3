using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraUpDownMovement : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public float sensitivity = 1.0f;
    public float returnSpeed = 1.0f;

    private float initialYAxisValue;

    private void Start()
    {
        initialYAxisValue = freeLookCamera.m_XAxis.Value;
    }

    private void Update()
    {
        float mouseY = Input.GetAxis("Mouse Y");

        // Adjust the camera's Y-axis rotation based on mouse input
        freeLookCamera.m_XAxis.Value += mouseY * sensitivity;

        // Return the camera to its initial Y-axis position


        while (Mathf.Abs(freeLookCamera.m_XAxis.Value - initialYAxisValue) > 0.01f)
        {
            freeLookCamera.m_XAxis.Value = Mathf.Lerp(freeLookCamera.m_XAxis.Value, initialYAxisValue, Time.deltaTime * returnSpeed);
            
        }
    }

  
}
