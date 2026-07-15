using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NableNative : MonoBehaviour
{
    public int Damage;

    public float RotateSpeed;
    public float RunSpeed;
    public float WalkSpeed;

    // Denna sätts nu dynamiskt via EnemiesHealth istället för att bara söka efter "Player"
    private Transform currentTarget; 

    public float AttackTime;
    float StartAttack;
    public LayerMask groundLayer;

    public Animator anime;
    public float PlayerDistance;
    public AudioClip[] SoundFx;
    public int SoundInt;
    public float PatrolTime;

    private bool patrollingRight = true;
    private float patrolTimer = 0f;
    private bool isWaiting = false;
    private Vector3 patrolStartPos;
    public float patrolDistance = 5f; // How far it patrols from the starting point
    public float JumpSpeed;
    public bool IsHang;
    public GameObject HangAble;
    bool IsHanging;
    public float AttackDistance;
    public bool IsHangEnemie;
    public Rigidbody rb;

    AudioSource sound;
    public AudioClip soundHang;

    public GameObject[] Mask;
    public bool IsPatroling = true;
    EnemiesHealth healt;
    
    public Transform landTarget;
    public bool isWater = false;

    void Start()
    {
        healt = GetComponent<EnemiesHealth>();
        sound = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        patrolStartPos = transform.position; // Set where patrolling begins
        StartAttack = AttackTime;
    }

    void Update()
    {
        // Hantera masker när vi tar skada
        if (healt != null && healt.maxHealth > healt.health)
        {
            foreach (GameObject masks in Mask)
            {
                if (masks != null)
                {
                    Destroy(masks);
                    Mask = null;
                }
            }
        }

        // Kontrollera om fienden är på marken
        bool isGrounded = Physics.CheckSphere(transform.position, 0.2f, groundLayer);

        if (isGrounded && !IsHanging)
        {
            anime.SetBool("Falling", false);
        }
        else if (!isGrounded && !IsHanging)
        {
            anime.SetBool("Falling", true);
        }

        // Hämta det aktuella målet direkt från EnemiesHealth!
        currentTarget = (healt != null) ? healt.currentTarget : null;

        if (IsHangEnemie)
        {
            if (IsHang)
            {
                float dis = Vector3.Distance(transform.position, HangAble.transform.position);

                // 1. OM HAN REDAN HÄNGER (Glid-fasen)
                if (IsHanging && HangAble != null)
                {
                    transform.position += transform.forward * RunSpeed * Time.deltaTime;

                    Quaternion targetRot = HangAble.transform.rotation;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotateSpeed * Time.deltaTime);

                    Vector3 targetPos = new Vector3(HangAble.transform.position.x, transform.position.y, HangAble.transform.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, 2f * Time.deltaTime);

                    // EXIT-CHECK
                    float dist = Vector3.Distance(transform.position, HangAble.transform.position);
                    if (dist > 15f) 
                    {
                        DismountHang();
                    }
                }
                // 2. OM HAN ÄR NÄRA LINAN OCH SKA HOPPA UPP
                else if (dis < 3.0f)
                {
                    anime.SetBool("Run", false);
                    anime.SetBool("Walking", false);
                    anime.SetBool("Hang", true);

                    rb.linearVelocity = (transform.forward * RunSpeed) + (Vector3.up * JumpSpeed);
                }
                // 3. SPRING MOT KANTEN
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, HangAble.transform.position, RunSpeed * Time.deltaTime);
                    anime.SetBool("Run", true);
                    anime.SetBool("Walking", false);
                }
            }
            else
            {
                // Kontrollera om vi har ett giltigt mål att jaga/attackera
                if (currentTarget != null)
                {
                    float dis = Vector3.Distance(transform.position, currentTarget.position);
                    transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

                    // --- ATTACKERA MÅLET ---
                    if (AttackDistance > dis)
                    {
                        AttackTime -= Time.deltaTime;
                        anime.SetBool("Run", false);
                        if (AttackTime < 0)
                        {
                            AttackTime = StartAttack;
                            Attack();
                        }
                    }

                    // --- JAGA MÅLET ---
                    if (dis < PlayerDistance)
                    {
                        anime.SetBool("Walking", false);

                        if (AttackDistance < dis)
                        {
                            IsHang = true;
                            if (!IsHanging)
                            {
                                if (IsPatroling)
                                {
                                    if (IsGroundAhead())
                                    {
                                        // Mark finns! Spring mot målet (spelaren eller infekterad fiende)
                                        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, RunSpeed * Time.deltaTime);
                                        anime.SetBool("Run", true);

                                        Vector3 direction = (currentTarget.position - transform.position).normalized;
                                        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                                        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                                    }
                                    else
                                    {
                                        // STUP!
                                        anime.SetBool("Run", false);
                                        anime.SetBool("Walking", true);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Om målet är för långt borta -> Patrullera
                        if (IsPatroling)
                        {
                            Patrol();
                            anime.SetBool("Run", false);
                        }
                    }
                }
                else
                {
                    // Inget mål -> Patrullera
                    if (IsPatroling)
                    {
                        Patrol();
                        anime.SetBool("Run", false);
                    }
                }
            }
        }

        // --- OM FIENDEN INTE SKA HÄNGA (IsHangEnemie == false) ---
        if (!IsHangEnemie)
        {
            if (currentTarget != null)
            {
                float dis = Vector3.Distance(transform.position, currentTarget.position);
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

                // --- ATTACKERA MÅLET ---
                if (AttackDistance > dis)
                {
                    AttackTime -= Time.deltaTime;
                    anime.SetBool("Run", false);
                    if (AttackTime < 0)
                    {
                        AttackTime = StartAttack;
                        Attack();
                    }
                }

                // --- JAGA MÅLET ---
                if (dis < PlayerDistance)
                {
                    isWater = true;
                    anime.SetBool("Walking", false);
                    
                    if (AttackDistance < dis)
                    {
                        if (IsGroundAhead())
                        {
                            // Spring mot målet
                            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, RunSpeed * Time.deltaTime);
                            anime.SetBool("Run", true);
                        }
                        else
                        {
                            // Stup! Backa undan
                            anime.SetBool("Run", false);
                            transform.position = Vector3.MoveTowards(transform.position, transform.position - transform.forward, WalkSpeed * Time.deltaTime);
                        }

                        // Titta mot målet
                        Vector3 direction = (currentTarget.position - transform.position).normalized;
                        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    if (IsPatroling)
                    {
                        Patrol();
                        anime.SetBool("Run", false);
                    }
                }
            }
            else
            {
                if (IsPatroling)
                {
                    Patrol();
                    anime.SetBool("Run", false);
                }
            }
        }
    }

    bool IsGroundAhead()
    {
        Vector3 forwardPos = transform.position + transform.forward * 0.5f;
        return Physics.Raycast(forwardPos + Vector3.up, Vector3.down, 2f, groundLayer);
    }

    void Patrol()
    {
        if (isWaiting)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= PatrolTime)
            {
                isWaiting = false;
                patrolTimer = 0f;
                patrollingRight = !patrollingRight; // Vänd om
            }
            anime.SetBool("Walking", false);
            return;
        }

        if (!IsGroundAhead())
        {
            isWaiting = true;
            patrolTimer = 0f;
            anime.SetBool("Walking", false);
            return; 
        }

        Vector3 targetPos = patrolStartPos + (patrollingRight ? Vector3.right : Vector3.left) * patrolDistance;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, WalkSpeed * Time.deltaTime);

        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
        }

        anime.SetBool("Walking", true);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            isWaiting = true;
            anime.SetTrigger("Looking");
        }
    }

    void DismountHang()
    {
        IsHang = false;
        sound.Stop();
        IsHanging = false;
        rb.useGravity = true; 
        IsHangEnemie = false;
        anime.SetBool("Hang", false);

        rb.AddForce((transform.forward + Vector3.up) * 5f, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemieHang"))
        {
            HangAble = other.gameObject;

            if (!IsHanging)
            {
                sound.clip = soundHang;
                sound.Play();
                anime.SetBool("Hang", true);
                IsHanging = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
            }
        }

        if (other.CompareTag("HangEnd"))
        {
            DismountHang();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("WaterExit") && isWater)
        {
            isWater = false;
            Destroy(other.gameObject, 0.1f);
            JumpOutOfWater();
        }
    }

    void JumpOutOfWater()
    {
        if (landTarget == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.useGravity = true;
        
        Vector3 targetPos = landTarget.position;
        Vector3 direction = (targetPos - transform.position);

        Vector3 horizontalDist = new Vector3(direction.x, 0, direction.z);
        float verticalLift = 8f; 
        float speedToCenter = 4f; 

        Vector3 finalJumpForce = horizontalDist.normalized * speedToCenter + Vector3.up * verticalLift;

        rb.AddForce(finalJumpForce, ForceMode.VelocityChange);
        transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));

        anime.SetTrigger("Jump");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WaterExit") && isWater)
        {
            isWater = false;
            Destroy(other.gameObject, 0.2f);
        }
    }

    void die()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().linearVelocity = -transform.forward * 15;
        this.enabled = false;
    }

    public void Attack()
    {
        anime.SetTrigger("Attack");
       
        // Om vårt mål är spelaren, skada spelaren.
        // Om vårt mål är en annan fiende (infektion), skada den fienden istället!
        if (currentTarget != null)
        {
            if (currentTarget.CompareTag("Player"))
            {
                Player playerComponent = currentTarget.GetComponent<Player>();
                if (playerComponent != null)
                {
                    playerComponent.TakeDamage(Damage);
                }
            }
            else if (currentTarget.CompareTag("Enemie"))
            {
                EnemiesHealth otherEnemy = currentTarget.GetComponent<EnemiesHealth>();
                if (otherEnemy != null)
                {
                    otherEnemy.TakeDamage(Damage);
                }
            }
        }
    }
}