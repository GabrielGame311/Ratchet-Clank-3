using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatController : MonoBehaviour
{

    public float MoveSpeed;
    public int Damage;
    public float RotateSpeed;
    public Animator anime;
    public float DistanceToSe;
    public float DistanceAttack;
    public float AttackTime = 2;
    GameObject Player;
    float StartAttack;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        StartAttack = AttackTime;
    }

    // Update is called once per frame
    void Update()
    {


        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);


        float dis = Vector3.Distance(transform.position, Player.transform.position);

        if (DistanceToSe > dis)
        {
            // Check if the player is outside of attack range
            if (DistanceAttack < dis)
            {
                // Start running toward the player
                anime.SetBool("Run", true);
                transform.LookAt(Player.transform);
                transform.position = Vector3.MoveTowards(transform.position, Player.transform.position, MoveSpeed * Time.deltaTime);
            }
            else
            {
                // Stop moving when within attack range
                anime.SetBool("Run", false);

                // Start the attack timer and attack if timer reaches zero
                AttackTime -= Time.deltaTime;
                if (AttackTime <= 0)
                {
                    Attack();
                    AttackTime = StartAttack; // Reset attack cooldown
                }
            }
        }
        else
        {
            // If the player is out of detection range, stop running
            anime.SetBool("Run", false);
        }



    }


    void Attack()
    {

        Player.GetComponent<QuarkController>().TakeDamage(Damage);
        anime.SetTrigger("Hit");
    }


}
