using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nefourius : MonoBehaviour
{
    public float MoveSpeed;
    public bool IsSecond;
    public static Nefourius Nefourius_;

    public Animator anime;

    public GameObject player;
    EnemiesHealth Health;
    public int Damage;
    public Transform WaypointMiddle;
    public Transform[] waypointNext;
    public Transform flypos;
    public int Point;
    bool ISRun = true;
    public float ShootSpeed;
    public float Wait;
    public bool Fly;
    bool IsAttack = false;
    public GameObject sphere;
    int  maxpoint;
    public float AttackDistance;
    bool Isflyed = false;
    public GameObject Prefab;
    float attacktime = 1;
    float startattack;
    bool attack = false;

    bool trigger = false;
    float attacks2 = 2;
    float startattacks2;
    public int DmgS;
    bool isint = false;
    bool hasJumped = false;

    bool isIncremented = false;
    public Transform Hand;
    public float JumpForce;
    public GameObject Granade;
    Rigidbody RB;
    bool incremented2 = false;
    public int pointShoot = 0;
    public int pointShoot2 = 0;
    public float ShootGranadeSpeed;

    public Transform Points;
    public Transform Boss2;
    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        anime = GetComponentInChildren<Animator>();
        Health = GetComponent<EnemiesHealth>();
        Nefourius_ = GetComponent<Nefourius>();
        player = GameObject.FindGameObjectWithTag("Player");
        startattack = attacktime;
        startattacks2 = attacks2;
        maxpoint = waypointNext.Length;
    }


    private void OnDestroy()
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = Points.transform.position;
        Boss2.gameObject.SetActive(true);

        player.GetComponent<CharacterController>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {

        if(IsAttack == false && trigger == false)
        {
            if (Vector3.Distance(transform.position, WaypointMiddle.position) > 1f)
            {
                // You've reached the waypoint or are close enough
                // Move towards the next waypoint or perform any desired actions
                ISRun = true;
            }
            else
            {
                ISRun = false;
                
                if(trigger == false)
                {

                    trigger = true;

                    IsAttack = true;
                }
            }

        }

        float dis = Vector3.Distance(transform.position, player.transform.position);

        if(pointShoot > 5)
        {
            DmgS = 1;
            pointShoot = 0;

        }
        if(pointShoot2 > 3)
        {
            DmgS = 2;
            pointShoot2 = 0;
        }


        if(IsAttack)
        {

            attacks2 -= Time.deltaTime;

            if(attacks2 < 0)
            {

                Attack2(DmgS);

                attacks2 = startattacks2;
            }


        }

        if(dis < AttackDistance)
        {
            attack = true;
        }
        else
        {
            attack = false;
        }

        if(attack)
        {

            attacktime -= Time.deltaTime;

            if(attacktime < 0)
            {
                attacktime = startattack;
                attacks();
            }
        }


        if (ISRun)
        {
            transform.position += (WaypointMiddle.position - transform.position).normalized * Time.deltaTime * MoveSpeed;
            anime.SetBool("Run", true);
        }
        else
        {
            
            anime.SetBool("Run", false);
        }


        if(Health.health < 5500)
        {
            if(Fly == false)
            {
                if(Isflyed == false)
                {
                   
                    StartCoroutine(Flys());
                   
                }


            }

           
        }
        if (Health.health < 9000)
        {
            // Increment DmgS only if it hasn't been incremented before
            if (!isIncremented)
            {
                DmgS++;

                // Set the boolean variable to true to indicate that the increment has occurred
                isIncremented = true;
            }
            
        }
        if(Health.health < 8000)
        {
            if(!incremented2)
            {
                DmgS++;

                incremented2 = true;
            }
        }
        if(DmgS == 2)
        {
            transform.LookAt(player.transform);
        }


        // Check if DmgS is greater than 0 and the jump animation has not been triggered
        if (DmgS > 0 && !hasJumped)
        {
            // Trigger the jump animation
            anime.SetTrigger("Jump");

            // Set the boolean variable to true to indicate that the jump animation has been triggered
            hasJumped = true;
        }

        if (DmgS > 0 && !isint)
        {
            Attack2(DmgS);
           
            isint = true;
        }


       

        if (Fly)
        {
            if (Point < waypointNext.Length)
            {

                if (Vector3.Distance(transform.position, waypointNext[Point].position) > 1)
                {
                    transform.position += (waypointNext[Point].position - transform.position).normalized * Time.deltaTime * MoveSpeed;
                }
                else
                {
                    //Point++;

                    if (Point > 5)
                    {
                        Fly = false;
                        Isflyed = true;
                        sphere.SetActive(false);
                        GetComponent<Rigidbody>().useGravity = true;
                        DmgS = 2;
                        IsAttack = true;
                    }
                }

                    
            }

           


        }

       // float dis = Vector3.Distance(transform.)

    }



    IEnumerator Flys()
    {
       
        yield return new WaitForSeconds(Wait);
        DmgS++;
        anime.SetInteger("Dmg", DmgS);
        sphere.SetActive(true);
        GetComponent<Rigidbody>().useGravity = false;
        transform.position = flypos.transform.position;
        IsAttack = false;
        Fly = true;
       

    }

    void attacks()
    {
        anime.SetTrigger("Attack");
        transform.LookAt(player.transform);

        player.GetComponent<Player>().TakeDamage(Damage);
        attack = false;

    }


    void Attack2(int dg)
    {
        
        dg = DmgS;

        anime.SetInteger("Dmg", dg);

        if (dg == 0)
        {
            GameObject prefabInstance = Instantiate(Prefab, transform.position, transform.rotation);

            prefabInstance.GetComponent<Rigidbody>().velocity = prefabInstance.transform.forward * ShootSpeed;
            transform.LookAt(player.transform);
            Destroy(prefabInstance, 3);
        }
        
        if (dg == 1)
        {

        }
       



    }


    public void JumpLeft()
    {
        // Assuming you want to jump left, adjust the force vector as needed
        Vector3 jumpForce = new Vector3(0f, 10f, 0f);

        // Apply the force to make the character jump left
        RB.AddForce(jumpForce, ForceMode.Impulse);

        // Start a coroutine to handle the left movement after a delay
        StartCoroutine(MoveLeftAfterJump());
    }

    public void JumpRight()
    {
        // Assuming you want to jump left, adjust the force vector as needed
        Vector3 jumpForce = new Vector3(0f, 10f, 0f);

        // Apply the force to make the character jump left
        RB.AddForce(jumpForce, ForceMode.Impulse);

        // Start a coroutine to handle the left movement after a delay
        StartCoroutine(MoveRightAfterJump());
    }

    IEnumerator MoveLeftAfterJump()
    {
        // Wait for a short duration (you can adjust this based on your needs)
        yield return new WaitForSeconds(0);

        // Move the character left after the jump
        // Assuming you want to move at a constant speed, adjust as needed
        float moveSpeed = 3f;
        Vector3 moveDirection = new Vector3(-2f, 0f, 0f);
        RB.velocity = moveDirection * moveSpeed;

        // You may want to reset the velocity or apply additional logic based on your game's requirements
        // For example, you might want to stop moving after a certain distance or time
    }

    IEnumerator MoveRightAfterJump()
    {
        // Wait for a short duration (you can adjust this based on your needs)
        yield return new WaitForSeconds(0);

        // Move the character left after the jump
        // Assuming you want to move at a constant speed, adjust as needed
        float moveSpeed = 3f;
        Vector3 moveDirection = new Vector3(2f, 0f, 0f);
        RB.velocity = moveDirection * moveSpeed;

        // You may want to reset the velocity or apply additional logic based on your game's requirements
        // For example, you might want to stop moving after a certain distance or time
    }

    public void ShootGranade()
    {
        
            GameObject prefabGr = Instantiate(Granade, Hand.transform.position, Quaternion.identity);

            prefabGr.GetComponent<Rigidbody>().AddForce(prefabGr.transform.up * ShootGranadeSpeed);
            prefabGr.transform.LookAt(player.transform);

        pointShoot++;
    }

    public void Shoot()
    {
        pointShoot2++;
    }

}
