using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float doubleJumpSpeed = 6.0f;
    public float glideGravity = 2.0f;
    public float normalGravity = 20.0f;
    public float glideSpeed = 3.0f;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    private bool canDoubleJump = false;
    private bool isGliding = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpSpeed;
                canDoubleJump = true;
            }

            isGliding = false; // Reset gliding when grounded
        }
        else
        {
            if (Input.GetButtonDown("Jump") && canDoubleJump)
            {
                moveDirection.y = doubleJumpSpeed;
                canDoubleJump = false;
            }

            if (Input.GetButton("Jump") && !controller.isGrounded && moveDirection.y <= 0)
            {
                isGliding = true;
            }

            if (isGliding)
            {
                moveDirection.y -= glideGravity * Time.deltaTime;
                moveDirection.x = Input.GetAxis("Horizontal") * glideSpeed;
                moveDirection.z = Input.GetAxis("Vertical") * glideSpeed;
                moveDirection = transform.TransformDirection(moveDirection);
            }
            else
            {
                moveDirection.y -= normalGravity * Time.deltaTime;
            }
        }

        controller.Move(moveDirection * Time.deltaTime);
    }
}
