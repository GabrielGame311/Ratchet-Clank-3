using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class RatchetController : MonoBehaviour
{
    // --- VARIABLER SOM DINA ANDRA SKRIPT KRÄVER (FIXAR ERRORS) ---
    public bool IsJump = false;
    public bool collision = false;
    public bool isSwimming = false; // Krävs av Swiming.cs
    public CinemachineFreeLook cine; // Krävs av IOSController.cs
    public void RatchetJump() { _directionY = JumpSpeed; isGliding = true; } // Krävs av IOSController.cs

    // --- ORIGINAL-VARIABLER ---
    public bool Ground = false;
    public static RatchetController RatchetController_;
    public Camera MyCam;
    public float Speed = 5f;
    public CharacterController MyController;
    public float Gravity;
    public float _directionY;
    public Animator anime;
    public bool Boots;
    public float bootsspeed;
    public GameObject particle_boot;
    public float JumpSpeed;
    public bool gravity = true;
    public bool ISHelikopter = false;
    public bool isGliding = false;
    public float FlySpeed;
    public bool doublejump = false;
    public float doublejumpMultiple = 0.5f;
    public PlayerControlls controls;
    public Transform cameraTransform;
    private float x, z;
    public bool CanMove = true;
    public bool StartJump = false;
    public float glideDescentSpeed = 1.5f;
    public float glideSmoothness = 2.0f;
    public float lastClickTime;
    [Header("Climbing Settings")]
    public bool isClimbing = false;
    public float climbCooldown = 0f;
    public float climbHorizontalSpeed = 3f;


    //JUMP

    float JumpHorizontalSpeed;
    bool IsJumping = false;
    float ySpeed;
    float jumpButtonGracePeriod;
    float? lastGroundedTime;
    float? jumpButtonPressedTime;
    float orginalStepOffset;

    void Start()
    {
        if (!cameraTransform) cameraTransform = Camera.main.transform;
        RatchetController_ = this;
        MyController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        
        // Försök hitta Cinemachine automatiskt om den inte är satt i inspektorn
        if (cine == null) cine = GameObject.FindObjectOfType<CinemachineFreeLook>();
    }

    private void Awake() { controls = new PlayerControlls(); }
    private void OnEnable() { controls.PlaystationControlls.Enable(); }
    private void OnDisable() { controls.PlaystationControlls.Disable(); }

    public void Jump(float Speed) { _directionY = Speed; }

    private void Update()
    {
        if (anime == null) anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        if (isGliding) Glide();
        if (climbCooldown > 0) climbCooldown -= Time.deltaTime;

        if (CanMove)
        {


           

            // Input hantering
            x = Input.GetAxisRaw("Horizontal");
            z = Input.GetAxisRaw("Vertical");
            Vector3 moveInput = new Vector3(x, 0, z).normalized;

            
            if (isClimbing)
            {
                HandleClimbingMovement();
            }
            else
            {
                anime.SetBool("IsHanging", false);
                anime.SetFloat("ClimbSpeed", 0);
                ApplyMovementAndGravity(moveInput);
            }

            // Hopp-indata (använder även IsJump för att synka med dina andra skript)

           


            MyController.Move(new Vector3(0, _directionY, 0) * Time.deltaTime);
            HandleBoots();
            HandleHelicopter();
            HandleStandardJump();

            if (MyController.isGrounded)
            {
                lastGroundedTime = Time.time;
                MyController.stepOffset = orginalStepOffset;

                // Sätt bools för markläge
                anime.SetBool("IsGrounded", true);
                anime.SetBool("IsJumping", false);
                anime.SetBool("IsFalling", false);
            }
            else
            {
                MyController.stepOffset = 0;
                anime.SetBool("IsGrounded", false);

                // Om karaktären rör sig nedåt i luften faller den
                if (_directionY < -1f)
                {
                    anime.SetBool("IsJumping", false);
                    anime.SetBool("IsFalling", true);
                    anime.SetBool("IsDoubleJump", false);
                }
            }
        }
    }


    
    void HandleClimbingMovement()
    {
        _directionY = 0;
        anime.SetBool("IsHanging", true);

        // 1. Hoppa av
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isClimbing = false;
            climbCooldown = 0.3f;
            _directionY = JumpSpeed;
            anime.SetBool("IsJumping", true);
            return;
        }

        // 2. Rörelse och Animation
        anime.SetFloat("ClimbSpeed", x); // Uppdaterar din Blend Tree (Left/Idle/Right)

        if (Mathf.Abs(x) > 0.1f)
        {
            float moveX = x * climbHorizontalSpeed * Time.deltaTime;

            // VIKTIG ÄNDRING: Vi flyttar upp strålen (Vector3.up * 1f) 
            // och kollar lite längre framför spelaren.
            Vector3 rayOrigin = transform.position + Vector3.up * 1.2f + (transform.right * x * 0.2f);

            // Rita ut strålen i Scene-vyn så du kan se om den träffar (Syns bara när du spelar)
            Debug.DrawRay(rayOrigin, transform.forward * 1.5f, Color.red);

            if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, 1.5f))
            {
                if (hit.collider.CompareTag("Trappa"))
                {
                    // Om strålen träffar en trappa, tillåt förflyttning
                    MyController.Move(transform.right * moveX);
                }
            }
        }
    }

    void HandleStandardJump()
    {

        if (Input.GetKeyDown(KeyCode.Space) && !isClimbing)
        {
            if (MyController.isGrounded || (lastGroundedTime != null && Time.time - lastGroundedTime.Value <= jumpButtonGracePeriod))
            {
                IsJump = true;
                _directionY = JumpSpeed;
                anime.SetBool("IsJumping", true);
                anime.SetBool("IsDoubleJump", false);
                // anime.SetBool("IsFalling", false);
                jumpButtonPressedTime = Time.time;

                doublejump = true;
                MyController.Move(Vector3.up * 0.1f);
            }
            else if (doublejump)
            {
                _directionY = JumpSpeed * doublejumpMultiple;
                // anime.SetTrigger("DoubleJump");
                anime.SetBool("IsDoubleJump", true);
                // anime.SetBool("IsJumping", true);
                // anime.SetBool("IsFalling", false);
                doublejump = false;
            }
        }
        ySpeed = _directionY;


    }

    void ApplyMovementAndGravity(Vector3 moveInput)
    {
        if (gravity && !MyController.isGrounded) _directionY -= Gravity * Time.deltaTime;
        else if (MyController.isGrounded) { _directionY = -2f; isGliding = false; doublejump = false; IsJump = false; }

        if (moveInput.magnitude >= 0.1f)
        {
            float tAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, tAngle, 0), 10f * Time.deltaTime);
            MyController.Move(transform.forward * Speed * Time.deltaTime);
            anime.SetBool("Run", true);
        }
        else anime.SetBool("Run", false);
    }

    private void Glide() { if (MyCam != null) MyController.Move(MyCam.transform.forward * FlySpeed * Time.deltaTime); }

    void HandleHelicopter()
    {
        if (!MyController.isGrounded && Input.GetKey(KeyCode.Space) && _directionY < 0 && !isClimbing)
        {
            ISHelikopter = true;
            _directionY = Mathf.Lerp(_directionY, -glideDescentSpeed, Time.deltaTime * glideSmoothness);
        }
        else
        {
            ISHelikopter = false;
        }

        // TILLAGD RAD: Skicka värdet till Animatorn. 
        // Kontrollera att parametern heter exakt "Helikopter" eller "Fly" i din Animator.
        //anime.SetBool("Glide", ISHelikopter);
        HelikopterController.Instance.StartHelikopter(ISHelikopter);
    }

    void HandleBoots()
    {
        // 1. Känner av dubbelklick på Shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Boots = true;
            lastClickTime = 2;
        }

        if(Boots)
        {
            lastClickTime -= Time.deltaTime;

            if(lastClickTime < 0)
            {
                lastClickTime = 2;
                Boots = false;

            }

        }

        // 2. Om man släpper Shift så stängs Boots av
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
           // Boots = false;
        }

        // 3. Utför rörelsen om Boots är aktiva
        if (Boots)
        {
            MyController.Move(transform.forward * bootsspeed * Time.deltaTime);

            // Uppdatera Animatorn och Partiklar
            if (anime != null) anime.SetBool("Boots", true);
            if (particle_boot != null) particle_boot.SetActive(true);
        }
        else
        {
            if (anime != null) anime.SetBool("Boots", false);
            if (particle_boot != null) particle_boot.SetActive(false);
        }
    }

    // Lägg till denna variabel högst upp i skriptet bland de andra
    

    // --- TRIGGERS (Viktigt för Trappa.cs) ---
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Trappas" && climbCooldown <= 0 && _directionY <= 0.5f)
        {
            //isClimbing = true;
            //_directionY = 0;
           // transform.forward = -other.transform.forward;
        }

        
    }

    

    // Dummy metoder för att undvika fler fel
    public void moves(InputAction.CallbackContext cn) { }
    public void DoubleJump(InputAction.CallbackContext n) { }
}