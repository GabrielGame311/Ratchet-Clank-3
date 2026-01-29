using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class RatchetController : MonoBehaviour
{
    // Existing variables remain unchanged
    public bool Ground = false;
    Transform FollowPlayer;
    Transform LookAtPlayer;
    private Vector3 velocity;
    public Hypershot hypershot;
    public bool isSwimming = false;
    public float swimSpeed;
    public static RatchetController RatchetController_;
    public Camera MyCam;
    public float Speed = 5f;
    public CharacterController MyController;
    public float RotationSpeed;
    float mDesiredRotation = 0f;
    public float Gravity;
    public float _directionY;
    public Animator anime;
    public bool Boots;
    public float bootsspeed;
    bool canjump = false;
    public GameObject particle_boot;
    Vector3 jump;
    bool holdclick = false;
    public float JumpSpeed;
    public float strafeSpeed = 4.0f;
    public bool gravity = true;
    public bool ISHelikopter = false;
    private bool lockStrafeMode = false;
    public bool IsJump = false;
    bool crouch = false;
    public bool doublejump = false;
    public float doublejumpMultiple = 0.5f;
    bool jumps;
    public bool collision = false;
    public float DownCollisionSpeed;
    public Transform target;
    public float trappaSpeed;
    public bool trappa;
    float clicked = 0;
    public float clicktime = 0;
    float clickdelay = 0.5f;
    IOSController IosController_;
    public PlayerControlls controls;
    Vector3 rotatedMovment;
    Vector3 movment;
    public CinemachineFreeLook cine;
    public Transform cameraTransform;
    private float x;
    private float z;
    public bool CanMove = true;
    bool IsJumpForward = false;
    public bool isGliding = false;
    public float FlySpeed;
    private float GlideSmoothness = 2.0f;
    public float GroundCheckDistance = 2f;
    public float buffer = 0.5f;
    public bool StartJump = false;
    float targetAngle;
    public CameraFollowHandler cameraTarget;
    private int groundLayer;
    private bool isGrounded;
    private Vector3 moveDirection;
    float StartClickTime_;
    public float HypershotGravity;
    [Header("Helicopter Settings")]
    public float glideMoveSpeed = 6.0f;
    public float glideDescentSpeed = 1.5f;
    public float glideSmoothness = 2.0f;
    // Climbing
    private bool isClimbing = false;
    public float climbSpeed = 3f;
    // New variables for ladder jumping
    public float jumpToLadderDistance = 2f; // Maximum distance to detect another ladder
    public float ladderJumpSpeed = 10f; // Speed of the jump to the next ladder
    private Transform targetLadder; // Target ladder to jump to

    void Start()
    {
        
        if (!cameraTransform) cameraTransform = Camera.main.transform;
        StartClickTime_ = clicktime;
        groundLayer = LayerMask.NameToLayer("Ground");
        LookAtPlayer = cine.LookAt;
        FollowPlayer = cine.Follow;
        RatchetController_ = GetComponent<RatchetController>();
        cine = GameObject.FindObjectOfType<CinemachineFreeLook>();
        MyController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        IosController_ = GameObject.FindObjectOfType<IOSController>();
    }



    private void Awake()
    {
        controls = new PlayerControlls();
    }

    private void OnEnable()
    {
        controls.PlaystationControlls.Movment.performed += moves;
        controls.PlaystationControlls.DoubleJump.performed += DoubleJump;
        controls.PlaystationControlls.Enable();
    }

    private void OnDisable()
    {
        controls.PlaystationControlls.DoubleJump.performed -= DoubleJump;
        controls.PlaystationControlls.Movment.performed -= moves;
        controls.PlaystationControlls.Disable();
    }

    public void Jump(float Speed)
    {
        _directionY = Speed;
    }

    private void Update()
    {
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        if (isGliding)
        {
            Glide();
        }

        if (CanMove)
        {
            Move2();
            if (collision == true)
            {
                // MyController.Move(transform.position = Vector3.down * DownCollisionSpeed * Time.deltaTime);
            }

            if (IosController_ != null)
            {
                x = IosController_.JoyStick_.Horizontal;
                z = IosController_.JoyStick_.Vertical;
            }
            else
            {
                x = Input.GetAxisRaw("Horizontal");
                z = Input.GetAxisRaw("Vertical");
            }
            Vector3 moveInput = new Vector3(x, 0, z).normalized;

            // Ladder climbing with Space key
            if (isClimbing && Input.GetKey(KeyCode.Space))
            {
                Vector3 climbMovement = Vector3.up * climbSpeed * Time.deltaTime;
                MyController.Move(climbMovement);
                _directionY = 0;
                gravity = false;
            }
            else if (isClimbing && !Input.GetKey(KeyCode.Space))
            {
                _directionY = 0;
                gravity = false;
            }
            else
            {
                gravity = true;
            }

            // Jump to another ladder with Space + Left Shift
            if (isClimbing && Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift))
            {
                JumpToNextLadder();
            }

            if (ISHelikopter)
            {
                _directionY = Mathf.Lerp(_directionY, -glideDescentSpeed, Time.deltaTime * glideSmoothness);
                Vector3 moveDirection = Vector3.zero;
                if (moveInput.magnitude >= 0.1f)
                {
                    Vector3 forward = cameraTransform.forward;
                    Vector3 right = cameraTransform.right;
                    forward.y = 0;
                    right.y = 0;
                    forward.Normalize();
                    right.Normalize();
                    moveDirection = (forward * z + right * x).normalized;
                    float targetAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                    Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                }
                Vector3 glideMovement = moveDirection * glideMoveSpeed + Vector3.up * _directionY;
                MyController.Move(glideMovement * Time.deltaTime);
                anime.SetBool("Run", false);
            }
            else
            {
                if (Input.GetMouseButtonDown(1))
                    lockStrafeMode = !lockStrafeMode;

                if (gravity)
                {
                    _directionY -= Gravity * Time.deltaTime;
                }

                if (moveInput.magnitude >= 0.1f)
                {
                    float targetAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                    Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                    moveDirection = targetRotation * Vector3.forward;
                    MyController.Move(moveDirection * Speed * Time.deltaTime);
                    anime.SetBool("Run", true);
                }
                else
                {
                    anime.SetBool("Run", false);
                }
            }

            if (controls.PlaystationControlls.Crouch.IsPressed())
            {
                anime.SetBool("Crouch", true);
            }
            else
            {
                anime.SetBool("Crouch", false);
            }

            if (Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
            {
                IsJump = true;
                StartJump = true;
                anime.SetTrigger("Jump");
                if (cameraTarget != null)
                {
                    cameraTarget.SetJumping(true);
                }
                doublejump = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (crouch)
                {
                    anime.SetBool("Crouch", false);
                    crouch = false;
                }
                else
                {
                    crouch = true;
                    anime.SetBool("Crouch", true);
                }
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                clicked++;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (clicked > 1)
                {
                    Boots = true;
                    clicktime = StartClickTime_;
                }
            }
            else
            {
                if (clicked > 1)
                {
                    clicktime -= Time.deltaTime;
                    if (clicktime < 0)
                    {
                        Boots = false;
                        clicked = 0;
                        clicktime = StartClickTime_;
                    }
                    else
                    {
                        Boots = true;
                    }
                }
            }

            if (Boots)
            {
                MyController.Move(transform.forward * bootsspeed * Time.deltaTime);
                anime.SetBool("Boots", true);
                particle_boot.SetActive(true);
            }
            else
            {
                anime.SetBool("Boots", false);
                particle_boot.SetActive(false);
            }

            if (gravity == true && !ISHelikopter)
            {
                jump.y = _directionY;
                MyController.Move(jump * Speed * Time.deltaTime);
            }
        }

        float minAirHeight = 2.0f;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            float distanceToGround = hit.distance;
            if (distanceToGround > minAirHeight)
            {
                StartJump = true;
            }
            else
            {
                StartJump = false;
            }
        }

        if (MyController.isGrounded)
        {
            isGrounded = true;
            doublejump = false;
            Ground = true;
            IsJump = false;
            IsJumpForward = false;
            HelikopterController.Instance.CancelHelikopter();
            ISHelikopter = false;
            if (gravity == true && ISHelikopter == false)
            {
                if (IsJumpForward == false)
                {
                    if (Input.GetKeyDown(KeyCode.Space) && !isClimbing)
                    {
                        anime.SetTrigger("Jump");
                        if (cameraTarget != null)
                        {
                            cameraTarget.SetJumping(true);
                        }
                        _directionY = JumpSpeed;
                        doublejump = true;
                        IsJump = true;
                        isGrounded = false;
                    }
                }

                if (controls.PlaystationControlls.Jump.IsPressed())
                {
                    isGrounded = false;
                    _directionY = JumpSpeed;
                    if (cameraTarget != null)
                    {
                        cameraTarget.SetJumping(true);
                    }
                    anime.SetTrigger("Jump");
                }
            }

            if (crouch == true)
            {
                jump.y = _directionY;
                MyController.Move(jump * Speed * Time.deltaTime);
                if (Input.GetKeyDown(KeyCode.Space) && Input.GetKey(KeyCode.D))
                {
                    _directionY = JumpSpeed;
                    if (cameraTarget != null)
                    {
                        cameraTarget.SetJumping(true);
                    }
                    anime.SetTrigger("JumpRight");
                }
            }
        }
        else
        {
            isGrounded = false;
            if (MyController.enabled)
            {
                Ground = false;
            }

            if (Input.GetKeyDown(KeyCode.Space) && doublejump && !isClimbing)
            {
                doublejump = false;
                IsJump = true;
                _directionY = JumpSpeed * doublejumpMultiple;
                anime.SetTrigger("DoubleJump");
                if (cameraTarget != null)
                {
                    cameraTarget.SetJumping(true);
                }
            }

            if (Input.GetKey(KeyCode.Space) && StartJump)
            {
                HelikopterController.Instance.StartHelikopter();
                _directionY -= FlySpeed * Time.deltaTime;
                ISHelikopter = true;
                if (cameraTarget != null)
                {
                    cameraTarget.SetJumping(false);
                }
            }
            else
            {
                HelikopterController.Instance.CancelHelikopter();
                ISHelikopter = false;
            }
        }

        if (isSwimming == true)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                MyController.Move(transform.position = Vector3.down * swimSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.Space))
            {
                MyController.Move(transform.up * swimSpeed * Time.deltaTime);
            }
        }
    }

    // New method to jump to the next ladder
    private void JumpToNextLadder()
    {
        // Use an overlap sphere to detect nearby ladders
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, jumpToLadderDistance);
        Transform closestLadder = null;
        float closestDistance = jumpToLadderDistance;

        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Trappa") && col.gameObject != MyController.transform.parent) // Avoid the current ladder
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLadder = col.transform;
                }
            }
        }

        if (closestLadder != null)
        {
            // Calculate direction to the target ladder
            Vector3 targetPosition = closestLadder.position;
            targetPosition.y = transform.position.y; // Keep the current height for a horizontal jump
            Vector3 jumpDirection = (targetPosition - transform.position).normalized;
            Vector3 jumpMovement = jumpDirection * ladderJumpSpeed * Time.deltaTime;

            // Move the character toward the target ladder
            MyController.Move(jumpMovement);

            // Check if the character has reached the target ladder
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                transform.position = new Vector3(targetPosition.x, transform.position.y, targetPosition.z); // Snap to ladder
                targetLadder = closestLadder; // Update the target ladder
                anime.SetTrigger("Jump"); // Trigger a jump animation
            }
        }
    }

    public bool IsGroundeds()
    {
        return MyController.isGrounded;
    }

    private void StartGlider()
    {
        isGliding = true;
    }

    private void Glide()
    {
        Vector3 gliderForward = MyCam.transform.forward;
        gliderForward.Normalize();
        float gliderSpeed = 5f;
        transform.position += gliderForward * gliderSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Space))
        {
            float ascendSpeed = 3f;
            transform.position += Vector3.up * ascendSpeed * Time.deltaTime;
        }
        float glideDrag = 1f;
        Vector3 velocityChange = -transform.forward * glideDrag * Time.deltaTime;
        transform.position += velocityChange;
    }

    private void StopGlider()
    {
        isGliding = false;
    }

    public void RatchetJump()
    {
        _directionY = JumpSpeed;
        StartGlider();
    }

    public void Move2()
    {
        // MyController.Move(rotatedMovment * Speed * Time.deltaTime);
    }

    public void DoubleJump(InputAction.CallbackContext n)
    {
        doublejump = false;
        _directionY = JumpSpeed * doublejumpMultiple;
        anime.SetTrigger("Jump");
    }

    public void moves(InputAction.CallbackContext cn)
    {
        if (rotatedMovment.magnitude > 0)
        {
            mDesiredRotation = Mathf.Atan2(rotatedMovment.x, rotatedMovment.z) * Mathf.Rad2Deg;
        }
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.2f))
        {
            return hit.collider.gameObject.layer == groundLayer;
        }
        return false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == groundLayer)
        {
            isGrounded = true;
            if (cameraTarget != null)
            {
                cameraTarget.SetJumping(false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trappa"))
        {
            isClimbing = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Trappa"))
        {
            isClimbing = false;
            gravity = true;
        }
    }

    IEnumerator waitboots()
    {
        if (Boots == true)
        {
            Boots = true;
        }
        yield return new WaitForSeconds(2);
        Boots = false;
    }

    // Draw the detection sphere in the editor for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, jumpToLadderDistance);
    }
}