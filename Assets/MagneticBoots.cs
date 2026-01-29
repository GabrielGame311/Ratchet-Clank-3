using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticBoots : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = 9.8f;
    public float wallDistance = 0.5f;
    public LayerMask wallLayer;

    private CharacterController controller;
    private Vector3 moveDirection;
    private Vector3 wallNormal;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            // Character is on the ground
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection) * moveSpeed;
        }
        else
        {
            // Character is in the air
            moveDirection.y -= gravity * Time.deltaTime;

            // Check for walls
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, controller.radius, transform.forward, out hit, wallDistance, wallLayer))
            {
                // Character is touching a wall
                wallNormal = hit.normal;

                // Calculate new move direction parallel to the wall
                moveDirection = Vector3.ProjectOnPlane(moveDirection, wallNormal);
                moveDirection += wallNormal * moveSpeed;
            }
        }

        // Move the character
        controller.Move(moveDirection * Time.deltaTime);
    }
}
