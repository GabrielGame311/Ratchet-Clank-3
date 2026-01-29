using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Jump : MonoBehaviour
{
    public float player3DSpeed = 5f;
    private bool IsGrounded = true;
    private const float groundY = (float)0.4;
    private Vector3 Velocity = Vector3.zero;
    public Vector3 gravity = new Vector3(0, -10, 0);

    void Start()
    {
    }

    void Update()
    {
        if (IsGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                IsGrounded = false;
                Velocity = new Vector3(Input.GetAxis("Horizontal"), 20, Input.GetAxis("Vertical"));
            }
            Velocity.x = Input.GetAxis("Horizontal");
            Velocity.z = Input.GetAxis("Vertical");
            if (Velocity.sqrMagnitude > 1)
                Velocity.Normalize();
            transform.position += Velocity * player3DSpeed * Time.deltaTime;
        }
        else
        {
            Velocity += gravity * Time.deltaTime;
            Vector3 position = transform.position + Velocity * Time.deltaTime;
            if (position.y < groundY)
            {
                position.y = groundY;
                IsGrounded = true;
            }
            transform.position = position;
        }
    }

}
