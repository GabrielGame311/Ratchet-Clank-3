using System.Collections;
using System.Collections.Generic;
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

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        patrolStartPos = transform.position; // Set where patrolling begins
        StartAttack = AttackTime;
        rb = GetComponent<Rigidbody>();
        Player_ = GameObject.FindGameObjectWithTag("Player");

    }

   

    // Update is called once per frame
    void Update()
    {


        if (IsHang)
        {


            float dis = Vector3.Distance(transform.position, HangAble.transform.position);

            if(dis < 1)
            {
               

               

                if(!IsHanging)
                {
                    anime.SetBool("Run", false);
                    anime.SetBool("Hang", true);
                    rb.AddForce(Vector3.up * JumpSpeed);
                }
                else
                {
                    rb.useGravity = false;
                    transform.position += transform.position * RunSpeed * Time.deltaTime;
                }

            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, HangAble.transform.position, RunSpeed * Time.deltaTime);
                anime.SetBool("Run", true);
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
            else
            {



                Patrol();
                anime.SetBool("Run", false);



            }
        }

        




        
    }

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

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "EnemieHang")
        {


            IsHanging = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "EnemieHang")
        {
            IsHanging = false;


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
