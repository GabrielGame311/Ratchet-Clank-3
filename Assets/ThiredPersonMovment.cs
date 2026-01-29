using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiredPersonMovment : MonoBehaviour
{

    public CharacterController controller;
    public float speed = 6f;

    public Transform Cam;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    public Animator anime;
    public ParticleSystem particlewrench;

    public float WrenchStrike;

    [SerializeField] private float _jumpSpeed = 8.0f;

    [SerializeField] private float _DoubleJumpMultipler = 0.5f;
    public float gravity = 9.8f;
    private Animator playeranime;
    private float _directionY;
    private bool _canDoubleJump = false;
    private bool jumped = false;

    Vector3  jump;


    private void Start()
    {
        playeranime = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical")*speed * Time.deltaTime;

        
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;


        if (direction.magnitude >= 0.1f)
        {




            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);




        }

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (jumped == true && _canDoubleJump)
            {
                _directionY = _jumpSpeed * WrenchStrike;
                anime.SetTrigger("Smashdown");
                anime.SetBool("Swing", false);
                _canDoubleJump = false;
                jumped = false;
                StartCoroutine(wait1());
                
            }
           
        }

        IEnumerator wait1()
        {
            yield return new WaitForSeconds(0.6f);
            particlewrench.Play();
        }
 


        if (controller.isGrounded)
        {
            anime.SetBool("Jump", false);
            _canDoubleJump = true;
          
           

            if (Input.GetButtonDown("Jump"))
            {
                _directionY = _jumpSpeed;
                jumped = true;
                anime.SetBool("Jump", true);

               
            }
           
            
        }
        else
        {
            if (Input.GetButtonDown("Jump") && _canDoubleJump)
            {
                
                _directionY = _jumpSpeed * _DoubleJumpMultipler;
                jumped = true;
                anime.SetTrigger("DubbelJump");
                _canDoubleJump = false;

            }
            
        }

        _directionY -= gravity * Time.deltaTime;
        jump.y = _directionY;
        controller.Move(jump * speed * Time.deltaTime);





        if (Input.GetKeyDown(KeyCode.Space) && Input.GetKeyDown(KeyCode.W))
        {
            anime.SetTrigger("JumpForward");
        }





            if (Input.GetKey(KeyCode.W))
            {
                anime.SetBool("Run", true);
                anime.SetBool("GunRun", true);
            }
            else
            {
                anime.SetBool("Run", false);
                anime.SetBool("GunRun", false);
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
