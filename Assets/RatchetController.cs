using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class RatchetController : MonoBehaviour
{
    // --- ALLA ORIGINAL-VARIABLER ---
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

    // Variabler som krävs av dina andra skript
    public bool IsJump = false;
    public bool collision = false;
    public bool isGliding = false;
    public float FlySpeed;
    private float GlideSmoothness = 2.0f;
    public bool trappa;
    public float trappaSpeed;
    public bool StartJump = false;

    public bool doublejump = false;
    public float doublejumpMultiple = 0.5f;
    public float DownCollisionSpeed;
    public Transform target;
    float clicked = 0;
    public float clicktime = 0;
    float clickdelay = 0.5f;

    IOSController IosController_;
    public PlayerControlls controls;
    public CinemachineFreeLook cine;
    public Transform cameraTransform;
    private float x;
    private float z;
    public bool CanMove = true;
    public float GroundCheckDistance = 2f;
    public float buffer = 0.5f;
    public CameraFollowHandler cameraTarget;
    private int groundLayer;
    float StartClickTime_;
    public float HypershotGravity;

    [Header("Helicopter Settings")]
    public float glideMoveSpeed = 6.0f;
    public float glideDescentSpeed = 1.5f;
    public float glideSmoothness = 2.0f;

    [Header("Climbing Settings")]
    public bool isClimbing = false;
    private float climbCooldown = 0f; // Hindrar att man fastnar direkt vid hopp

    void Start()
    {
        if (!cameraTransform) cameraTransform = Camera.main.transform;
        StartClickTime_ = clicktime;
        groundLayer = LayerMask.NameToLayer("Ground");
        RatchetController_ = GetComponent<RatchetController>();
        MyController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        IosController_ = GameObject.FindObjectOfType<IOSController>();
    }

    private void Awake() { controls = new PlayerControlls(); }
    private void OnEnable() { controls.PlaystationControlls.Enable(); }
    private void OnDisable() { controls.PlaystationControlls.Disable(); }

    public void Jump(float Speed) { _directionY = Speed; }
    public void RatchetJump() { _directionY = JumpSpeed; isGliding = true; }

    private void Update()
    {
        if (anime == null) anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();

        // Kör Glide-rörelse om den är aktiv
        if (isGliding) Glide();

        // Minska cooldown för klättring
        if (climbCooldown > 0) climbCooldown -= Time.deltaTime;

        if (CanMove)
        {
            x = (IosController_ != null) ? IosController_.JoyStick_.Horizontal : Input.GetAxisRaw("Horizontal");
            z = (IosController_ != null) ? IosController_.JoyStick_.Vertical : Input.GetAxisRaw("Vertical");
            Vector3 moveInput = new Vector3(x, 0, z).normalized;

            // --- TRAPPA LOGIK ---
            if (isClimbing)
            {
                if (_directionY < 0) _directionY = 0;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    PerformClimbJump();
                }
                anime.SetBool("IsHanging", true);
            }
            else
            {
                anime.SetBool("IsHanging", false);
                // Vanlig gravitation och rörelse
                ApplyMovementAndGravity(moveInput);
            }

            // --- HOPP & DOUBLE JUMP ---
            if (Input.GetKeyDown(KeyCode.Space) && !isClimbing)
            {
                HandleStandardJump();
            }

            // Applicera all vertikal rörelse
            MyController.Move(new Vector3(0, _directionY, 0) * Time.deltaTime);

            HandleBoots();
            HandleHelicopter();
        }

        // Check för helikopter-start (raycast)
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit))
            StartJump = hit.distance > 2.0f;
    }

    void PerformClimbJump()
    {
        isClimbing = false;
        climbCooldown = 0.2f; // Ger spelaren tid att lämna triggern
        _directionY = JumpSpeed;
        anime.SetTrigger("Jump");
    }

    void HandleStandardJump()
    {
        // Vi kollar både isGrounded och om _directionY är nära 0 (vilket betyder att vi står still)
        bool isActuallyGrounded = MyController.isGrounded || (_directionY < 0 && _directionY > -3f);

        if (isActuallyGrounded)
        {
            _directionY = JumpSpeed;
            anime.SetTrigger("Jump");
            doublejump = true;
            // Vi tvingar CharacterController att förstå att vi lämnar marken
            MyController.Move(Vector3.up * 0.1f);
        }
        else if (doublejump)
        {
            _directionY = JumpSpeed * doublejumpMultiple;
            anime.SetTrigger("DoubleJump");
            doublejump = false;
        }
    }

    void ApplyMovementAndGravity(Vector3 moveInput)
    {
        if (gravity && !MyController.isGrounded) _directionY -= Gravity * Time.deltaTime;
        else if (MyController.isGrounded) { _directionY = -2f; isGliding = false; doublejump = false; }

        if (moveInput.magnitude >= 0.1f)
        {
            float tAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, tAngle, 0), 10f * Time.deltaTime);
            MyController.Move(transform.forward * Speed * Time.deltaTime);
            anime.SetBool("Run", true);
        }
        else anime.SetBool("Run", false);
    }

    private void Glide()
    {
        if (MyCam != null)
            MyController.Move(MyCam.transform.forward * FlySpeed * Time.deltaTime);
    }

    void HandleHelicopter()
    {
        if (!MyController.isGrounded && Input.GetKey(KeyCode.Space) && _directionY < 0 && !isClimbing)
        {
            ISHelikopter = true;
            _directionY = Mathf.Lerp(_directionY, -glideDescentSpeed, Time.deltaTime * glideSmoothness);
            if (HelikopterController.Instance) HelikopterController.Instance.StartHelikopter();
        }
        else
        {
            ISHelikopter = false;
            if (HelikopterController.Instance) HelikopterController.Instance.CancelHelikopter();
        }
    }

    void HandleBoots()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) clicked++;
        if (Input.GetKey(KeyCode.LeftShift) && clicked > 1) Boots = true;
        else if (clicked > 1)
        {
            clicktime -= Time.deltaTime;
            if (clicktime < 0) { Boots = false; clicked = 0; clicktime = StartClickTime_; }
            else Boots = true;
        }
        if (Boots) { MyController.Move(transform.forward * bootsspeed * Time.deltaTime); anime.SetBool("Boots", true); particle_boot.SetActive(true); }
        else { anime.SetBool("Boots", false); particle_boot.SetActive(false); }
    }

    // --- TRIGGERS ---
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trappa") && climbCooldown <= 0 && _directionY <= 0.5f)
        {
            isClimbing = true;
            _directionY = 0;
            isGliding = false;
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Trappa") && climbCooldown <= 0 && _directionY <= 0)
            isClimbing = true;
    }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Trappa")) isClimbing = false; }

    // --- DUMMY METODER ---
    public void moves(InputAction.CallbackContext cn) { }
    public void DoubleJump(InputAction.CallbackContext n) { }
    public void Move2() { }
   
}