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


    public Animator anime;
    public float PlayerDistance;

    public float PatrolTime;

    private bool patrollingRight = true;
    private float patrolTimer = 0f;
    private bool isWaiting = false;
    private Vector3 patrolStartPos;
    public float patrolDistance = 5f; // How far it patrols from the starting point



    public float AttackDistance;



    // Start is called before the first frame update
    void Start()
    {
        patrolStartPos = transform.position; // Set where patrolling begins
        StartAttack = AttackTime;

        Player_ = GameObject.FindGameObjectWithTag("Player");

    }

    // Update is called once per frame
    void Update()
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

                transform.position = Vector3.MoveTowards(transform.position, Player_.transform.position, RunSpeed * Time.deltaTime);

                Vector3 direction = (Player_.transform.position - transform.position).normalized;

                // Ber�kna m�lrotationen med riktningen mot spelaren
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                // Roterar objektet l�ngsamt mot spelaren med RotateTowards
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);

                anime.SetBool("Run", true);
            }

                





        }
        else
        {

            Patrol();
            anime.SetBool("Run", false);
        }




        
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


    void die()
    {
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().linearVelocity = -transform.forward * 15;


    }


    public void Attack()
    {

        anime.SetTrigger("Attack");
       
        Player_.GetComponent<Player>().TakeDamage(Damage);
    }

}
