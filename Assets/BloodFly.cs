using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodFly : MonoBehaviour
{

    public Animator anime;

    public float RotateSpeed;
    public float RotateSpeedPlayer;

    public float MoveSpeed;
    public float MoveSpeedPlayer;

    public float AttackTime;
    float startTime;

    public float TakeDamage;
    public float PlayerDistanceAttack;

    //SePlayer
    public float SePlayerDistance;

    GameObject Player_;
    public float DieSpeed;
    public int pointsCount = 12;

    public float radius = 3f;

    private Vector3[] circlePoints;
    private int currentIndex = 0;
    private Vector3 centerPoint;
    private Vector3 previousDirection;
    private float angle = 0f;
    public float maxBankAngle = 45f;
    public float bankSmooth = 2f;

    public float waitAtCenterTime = 2f;  // seconds to wait at center before circling
    private bool reachedCenter = false;
    private float waitTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        centerPoint = transform.position;
        previousDirection = transform.forward;
        
        startTime = AttackTime;

        Player_ = GameObject.FindGameObjectWithTag("Player");

    }

    // Update is called once per frame
    void Update()
    {


        float dis = Vector3.Distance(transform.position, Player_.transform.position);


        if(PlayerDistanceAttack > dis)
        {
            AttackTime -= Time.deltaTime;

            if (AttackTime < 0)
            {

                AttackTime = startTime;

                Attack();
            }

        }

        if(GetComponent<EnemiesHealth>().health == 0)
        {
            Die();
        }

        if (dis < SePlayerDistance)
        {





            if (PlayerDistanceAttack < dis)
            {




                transform.position = Vector3.MoveTowards(transform.position, Player_.transform.position, MoveSpeedPlayer * Time.deltaTime);



                Vector3 direction = (Player_.transform.position - transform.position).normalized;

                // Beräkna målrotationen med riktningen mot spelaren
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                // Roterar objektet långsamt mot spelaren med RotateTowards
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeedPlayer * Time.deltaTime);





            }

        }


       
            float disToPlayer = Vector3.Distance(transform.position, Player_.transform.position);
            float distanceToCenter = Vector3.Distance(transform.position, centerPoint);

            if (disToPlayer > SePlayerDistance)
            {
                // Player is far → move back to center + wait + circle

                if (!reachedCenter)
                {
                    // Move to center
                    if (distanceToCenter > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, centerPoint, MoveSpeed * Time.deltaTime);

                        Vector3 directionToCenter = (centerPoint - transform.position).normalized;
                        if (directionToCenter != Vector3.zero)
                        {
                            Quaternion lookRotation = Quaternion.LookRotation(directionToCenter);
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                        }
                    }
                    else
                    {
                        reachedCenter = true;
                        waitTimer = 0f;
                    }
                }
                else
                {
                    // Wait at center before circling
                    if (waitTimer < waitAtCenterTime)
                    {
                        waitTimer += Time.deltaTime;
                        // Idle or do nothing
                    }
                    else
                    {
                        // Start circling
                        angle += MoveSpeed * Time.deltaTime;
                        float x = Mathf.Cos(angle) * radius;
                        float z = Mathf.Sin(angle) * radius;
                        Vector3 nextPos = centerPoint + new Vector3(x, 0, z);
                        transform.position = nextPos;

                        Vector3 moveDir = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle)).normalized;

                        float turnAmount = Vector3.SignedAngle(previousDirection, moveDir, Vector3.up) / 90f;
                        float bank = Mathf.Clamp(-turnAmount * maxBankAngle, -maxBankAngle, maxBankAngle);

                        if (moveDir != Vector3.zero)
                        {
                            Quaternion lookRot = Quaternion.LookRotation(moveDir);
                            Quaternion bankRot = lookRot * Quaternion.Euler(0, 0, bank);
                            transform.rotation = Quaternion.Slerp(transform.rotation, bankRot, RotateSpeed * Time.deltaTime);
                        }

                        previousDirection = moveDir;
                    }
                }
            }
            else
            {
                // Player is close → chase or attack
                reachedCenter = false; // reset so it moves back next time player is far

                // Your existing chasing/attacking code here
            }
        



    }




    void Die()
    {
       // transform.position += transform.forward * DieSpeed * Time.deltaTime;
        GetComponent<Rigidbody>().velocity = -transform.forward * DieSpeed;
        anime.transform.Rotate(0, 0, +500 * Time.deltaTime);

    }

    public void Attack()
    {

        Player_.GetComponent<Player>().TakeDamage(((int)TakeDamage));
        anime.SetTrigger("Attack");
    }

}
