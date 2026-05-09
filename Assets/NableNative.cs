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

    GameObject Player_;

    public float AttackTime;

    float StartAttack;
    public LayerMask groundLayer;

    public Animator anime;
    public float PlayerDistance;

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
    // Start is called before the first frame update
    public Transform landTarget;
    public bool isWater = false;
    void Start()
    {
        healt = GetComponent<EnemiesHealth>();
        sound = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        patrolStartPos = transform.position; // Set where patrolling begins
        StartAttack = AttackTime;
        
        Player_ = GameObject.FindGameObjectWithTag("Player");

    }

   

    // Update is called once per frame
    void Update()
    {

        if(healt.maxHealth > healt.health)
        {
            foreach(GameObject masks in Mask)
            {
                if(masks != null)
                {



                    Destroy(masks);
                    Mask = null;
                }
            }
        }

        // Kontrollera om fienden är på marken (använd din befintliga GroundLayer)
        bool isGrounded = Physics.CheckSphere(transform.position, 0.2f, groundLayer);

        if (isGrounded && !IsHanging)
        {
            // Om vi har landat, gå tillbaka till vanlig Idle/Run
            anime.SetBool("Falling", false);
        }
        else if (!isGrounded && !IsHanging)
        {
            // Om vi är i luften men inte hänger, spela fall-animationen
            anime.SetBool("Falling", true);
        }

       
        if (IsHangEnemie)
        {

            if (IsHang)
            {
                float dis = Vector3.Distance(transform.position, HangAble.transform.position);

                // 1. OM HAN REDAN HÄNGER (Glid-fasen)
                if (IsHanging && HangAble != null)
                {
                    // 1. RÖRELSE: Åk framåt med den hastighet du satt (7)
                    // Vi använder fiendens egna framåtriktning som vi roterar mjukt
                    transform.position += transform.forward * RunSpeed * Time.deltaTime;

                    // 2. MJUK ROTATION: Sväng mot linans riktning istället för att snappa
                    // RotateSpeed (125) gör att han följer kurvor snyggt
                    Quaternion targetRot = HangAble.transform.rotation;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotateSpeed * Time.deltaTime);

                    // 3. MJUK CENTRERING (Viktigast!): 
                    // Istället för att spawna på linan, drar vi honom sakta mot mitten i sidled
                    Vector3 targetPos = new Vector3(HangAble.transform.position.x, transform.position.y, HangAble.transform.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, 2f * Time.deltaTime);


                    // 3. EXIT-CHECK: 
                    // Om vi kommit för långt bort från linans startpunkt, hoppa av.
                    float dist = Vector3.Distance(transform.position, HangAble.transform.position);
                    if (dist > 15f) // Justera distansen efter hur långa dina linor är
                    {
                        DismountHang();
                    }
                }
                // 2. OM HAN ÄR NÄRA LINAN OCH SKA HOPPA UPP
                else if (dis < 3.0f) // Justera för att starta hoppet tidigare från kanten
                {
                    // Titta mot linan så hoppet blir rätt
                    Vector3 dir = (HangAble.transform.position - transform.position).normalized;
                    // transform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));

                    // Starta animationen: Vi sätter Hang-boolen till true
                    anime.SetBool("Run", false);
                    anime.SetBool("Walking", false);
                    anime.SetBool("Hang", true);
                    // Detta kommer att trigga övergången: Start -> Any State -> StartHang

                    // Ge honom kraft framåt och uppåt mot linan
                    rb.linearVelocity = (transform.forward * RunSpeed) + (Vector3.up * JumpSpeed);

                    // Vi sätter IsHanging till true i OnTriggerEnter när han nuddar linan
                }
                // 3. SPRING MOT KANTEN
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, HangAble.transform.position, RunSpeed * Time.deltaTime);
                    anime.SetBool("Run", true);
                    anime.SetBool("Walking", false);

                    // Vector3 dir = (HangAble.transform.position - transform.position).normalized;
                    // transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)), RotateSpeed * Time.deltaTime);
                }
            }
            else
            {
                float dis = Vector3.Distance(transform.position, Player_.transform.position);

                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

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
                if (GetComponent<EnemiesHealth>().health == 0)
                {
                    die();
                }

                if (dis < PlayerDistance)
                {

                    anime.SetBool("Walking", false);

                    
                    
                        if (AttackDistance < dis)
                        {
                            IsHang = true;
                            if (!IsHanging)
                            {


                                if(IsPatroling)
                                {
                                    if (IsGroundAhead())
                                    {
                                        // Mark finns! Spring mot spelaren
                                        transform.position = Vector3.MoveTowards(transform.position, Player_.transform.position, RunSpeed * Time.deltaTime);
                                        anime.SetBool("Run", true);
                                    }
                                    else
                                    {
                                        // STUP! Istället för att stå still, låt oss backa lite eller sluta springa
                                        anime.SetBool("Run", false);
                                        anime.SetBool("Walking", true);

                                        // Backa långsamt bort från kanten
                                        transform.position = Vector3.MoveTowards(transform.position, transform.position - transform.forward, WalkSpeed * Time.deltaTime);

                                        // Valfritt: Få den att se sig omkring (Looking animation)
                                        // anime.SetTrigger("Looking"); 
                                    }

                                    // Rotera fortfarande mot spelaren så den ser arg ut vid kanten
                                    Vector3 direction = (Player_.transform.position - transform.position).normalized;
                                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                                }

                               
                            }


                        }
                     

                   








                }
                else
                {



                    if(IsPatroling)
                    {
                        Patrol();
                        anime.SetBool("Run", false);
                    }



                }
            }
        }

        if(!IsHangEnemie)
        {

            float dis = Vector3.Distance(transform.position, Player_.transform.position);

            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

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
            if (GetComponent<EnemiesHealth>().health == 0)
            {
                die();
            }

            if (dis < PlayerDistance)
            {
                isWater = true;
                anime.SetBool("Walking", false);
                if (AttackDistance < dis)
                {
                    if (IsGroundAhead())
                    {

                        // Mark finns! Spring mot spelaren
                        transform.position = Vector3.MoveTowards(transform.position, Player_.transform.position, RunSpeed * Time.deltaTime);
                        anime.SetBool("Run", true);
                    }
                    else
                    {
                        // STUP! Istället för att stå still, låt oss backa lite eller sluta springa
                        anime.SetBool("Run", false);
                        //anime.SetBool("Walking", true);

                        // Backa långsamt bort från kanten
                        transform.position = Vector3.MoveTowards(transform.position, transform.position - transform.forward, WalkSpeed * Time.deltaTime);

                        // Valfritt: Få den att se sig omkring (Looking animation)
                        // anime.SetTrigger("Looking"); 
                    }

                    // Rotera fortfarande mot spelaren så den ser arg ut vid kanten
                    Vector3 direction = (Player_.transform.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                }








            }
            else
            {



                if(IsPatroling)
                {
                    Patrol();
                    anime.SetBool("Run", false);
                }



            }
        }

        

        




        
    }

    // Inuti din if (IsHanging) i Update:
    

    bool IsGroundAhead()
    {
        // Skjut en stråle från en punkt framför fienden och neråt
        // transform.forward * 0.5f gör att vi kollar ca en halv meter framför fötterna
        Vector3 forwardPos = transform.position + transform.forward * 0.5f;

        // Vi startar strålen 1 meter upp (Vector3.up) för att vara säkra på att den inte börjar under marken
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
                patrollingRight = !patrollingRight; // Turn around
            }
            anime.SetBool("Walking", false);
            return;
        }



        if (!IsGroundAhead())
        {
            isWaiting = true;
            patrolTimer = 0f;
            anime.SetBool("Walking", false);
            return; // Avbryt rörelsen här så den inte går över kanten
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
        rb.useGravity = true; // Slå på gravitationen så han faller ner
        IsHangEnemie = false;
        // Uppdatera Animator:
        // Vi stänger av "Hang"-boolen, vilket kommer trigga dina utgångar.
        // Se till att övergångarna från HangIdle -> HangExiit 
        // och HangExiit -> HangFall är korrekt inställda med 'Has Exit Time'.
        anime.SetBool("Hang", false);

        // Ge en liten knuff framåt/neråt när man hoppar av
        rb.AddForce((transform.forward + Vector3.up) * 5f, ForceMode.Impulse);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemieHang"))
        {
            // Vi sparar bara den nya linan för att få dess riktning (Z-axel)
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

        // I din OnTriggerEnter
        if (other.CompareTag("HangEnd"))
        {
            DismountHang();
           // Destroy(HangAble);
        }


        


    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("WaterExit") && isWater)
        {
            isWater = false;
            Destroy(other.gameObject, 01);
            JumpOutOfWater();
        }
    }

    

    void JumpOutOfWater()
    {
        if (landTarget == null) return;

        // 1. Nollställ all fart under vatten så hoppet blir super-snappy
        rb.linearVelocity = Vector3.zero;
        rb.useGravity = true;
        
        // 2. Räkna ut riktningen till mitten av marken
        Vector3 targetPos = landTarget.position;
        Vector3 direction = (targetPos - transform.position);

        // Vi delar upp kraften: 
        // Horisontell (X, Z) för att nå fram snabbt
        Vector3 horizontalDist = new Vector3(direction.x, 0, direction.z);
        // Vertikal (Y) för att komma UPP ur vattnet snabbt
        float verticalLift = 8f; // Öka denna om han är djupt under ytan
        float speedToCenter = 4f; // Öka denna för att han ska flyga snabbare framåt

        Vector3 finalJumpForce = horizontalDist.normalized * speedToCenter + Vector3.up * verticalLift;

        // 3. Skjut iväg! VelocityChange struntar i Mass och ger omedelbar fart
        rb.AddForce(finalJumpForce, ForceMode.VelocityChange);

        // 4. Se till att han tittar mot mitten direkt
        transform.LookAt(new Vector3(targetPos.x, transform.position.y, targetPos.z));

        anime.SetTrigger("Jump");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemieHang")
        {
            //IsHanging = false;


        }

        if (other.CompareTag("WaterExit") && isWater)
        {
            isWater = false;
            Destroy(other.gameObject, 02);
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
       
        Player_.GetComponent<Player>().TakeDamage(Damage);
    }

}
