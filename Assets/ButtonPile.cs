using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPile : MonoBehaviour
{

    public float RotateSpeed; // Speed of rotation in degrees per second
    public float XRotate; // Target rotation angle in degrees
    private bool IsTrigger = false; // Determines if rotation is active
    public Transform ArrowObj; // Reference to the object to rotate
    public Transform Door;
    private float currentRotation = 0f; // Tracks the current rotation amount

    void Update()
    {
        if (IsTrigger)
        {
            // Calculate rotation to apply for this frame
            float rotationStep = RotateSpeed * Time.deltaTime;

            // Check if adding this step would exceed the target rotation
            if (currentRotation + rotationStep >= XRotate)
            {
                // Clamp rotation to the target angle
                rotationStep = XRotate - currentRotation;
                ArrowObj.GetComponent<Animator>().SetTrigger("Trigger");
                Door.GetComponentInChildren<Animator>().SetTrigger("Open");
                Destroy(GetComponent<ButtonPile>());
                IsTrigger = false; // Stop further rotation
            }

            // Apply the rotation and update the current rotation amount
            ArrowObj.transform.Rotate(rotationStep, 0, 0);
            currentRotation += rotationStep;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            IsTrigger = true; // Start the rotation
            
            currentRotation = 0f; // Reset the tracked rotation
            GetComponent<SphereCollider>().enabled = false; // Disable trigger to prevent retriggering
        }
    }

}
