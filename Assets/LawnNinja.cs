using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LawnNinja : MonoBehaviour
{

    public float PlayerDistance;
    public float SePlayerDistance;
    public Animator anime;
    public bool sePlayer = false;
    public float SpeedMove;
    public float AttackTime;
    float startTimeAttack;

    public float TakeDamage;

    GameObject player_;

    // Start is called before the first frame update
    void Start()
    {
        startTimeAttack = AttackTime;

        player_ = GameObject.FindGameObjectWithTag("Player");


    }

    // Update is called once per frame
    void Update()
    {

        float dis = Vector3.Distance(transform.position, player_.transform.position);


        if(dis < SePlayerDistance)
        {
            sePlayer = true;
            transform.LookAt(player_.transform);
            anime.SetBool("Run", true);

           
        }
        else
        {
            sePlayer = false;
            anime.SetBool("Run", false);

        }

        if(sePlayer)
        {
            if (PlayerDistance < dis)
            {

                transform.position = Vector3.MoveTowards(transform.position, player_.transform.position, SpeedMove * Time.deltaTime);

                
            }
            else
            {
                anime.SetBool("Run", false);
                AttackTime -= Time.deltaTime;
                if (AttackTime < 0)
                {

                    AttackTime = startTimeAttack;
                    Attack();
                }
            }

        }
       

    }



    public void Attack()
    {
        anime.SetTrigger("Hit");
        player_.GetComponent<Player>().TakeDamage(((int)TakeDamage));
    }
}
