using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Movment : MonoBehaviour
{
    private GameObject armor;
    private GameObject Player;
    public float jumpSpeed = 8.0f;
    public float jumpForce = 8f;
    public Animator anime;
    public float gravity = 9.8f;
    CharacterController Controller;
    public AudioSource wrnechsound;

    private Vector3 moveDir = Vector3.zero;
    public float Speed;

    public Transform Cam;

    

    // Start is called before the first frame update
    void Start()
    {
        
        Controller = GetComponent<CharacterController>();

        Player = GameObject.FindGameObjectWithTag("Ratchet");






      

    }

    // Update is called once per frame
    void Update()
    {
        
         float Horizontal = Input.GetAxis("Horizontal") * Speed * Time.deltaTime;
         float Vertical = Input.GetAxis("Vertical") * Speed * Time.deltaTime;

        Vector3 Movement = Cam.transform.right * Horizontal + Cam.transform.forward * Vertical;
        Movement.y = 0f;
        if (!Controller.isGrounded)
        {
            

            if (Input.GetButtonDown("Jump"))
            {
                moveDir.y = jumpForce;
                anime.SetBool("Jump", true);

            }
            else
            {
                anime.SetBool("Jump", false);
            } 
            if (Input.GetKeyDown(KeyCode.Space))
            {
                anime.SetTrigger("Jump2");
            }



        }

        moveDir.y -= gravity * Time.deltaTime;
        Controller.Move(moveDir * Time.deltaTime);
        
        





        Controller.Move(Movement);

        if (Movement.magnitude != 0f)
        {
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Cam.GetComponent<CameraMove>().sensivity * Time.deltaTime);


            Quaternion CamRotation = Cam.rotation;
            CamRotation.x = 0f;
            CamRotation.z = 0f;

            transform.rotation = Quaternion.Lerp(transform.rotation, CamRotation, 0.1f);

        }



        






        if (Input.GetKey(KeyCode.W))
        {
            anime.SetBool("Run",true);
        }
        else
        {
            anime.SetBool("Run", false);
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            anime.SetBool("RunBack", true);
        }
        else
        {
            anime.SetBool("RunBack", false);
        }
       
        if (Input.GetKey(KeyCode.A))
        {
            anime.SetBool("RunLeft", true);
        }
        else
        {
            anime.SetBool("RunLeft", false);
        }
       
        if (Input.GetKey(KeyCode.D))
        {
            anime.SetBool("RunRight", true);
        }
        else
        {
            anime.SetBool("RunRight", false);
        }

       





    }
}
