using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossThyrra : MonoBehaviour
{
    public float AttackTime;
    float startTime;
    public float DistanceAttack;
    public float DistanceSeplayer;
    public int Damage;
    public float MoveSpeed;
    Animator anime;

    bool IsAttacking = false;
    public GameObject player;
    public GameObject HealthCanvas;


    



    // Start is called before the first frame update
    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player");
        anime = GetComponentInChildren<Animator>();
        HealthCanvas.SetActive(true);
        startTime = AttackTime;
    }

    // Update is called once per frame
    void Update()
    {
        

        float dis = Vector3.Distance(transform.position, player.transform.position);
        
        if (DistanceSeplayer < dis)
        {
            transform.LookAt(player.transform);
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, Time.deltaTime * MoveSpeed);
           
            anime.SetBool("Run", true);
           
              
            


        }
        if(dis < DistanceAttack)
        {
            AttackTime -= Time.deltaTime;

            if (AttackTime < 0)
            {
                TakeDamage();
                AttackTime = startTime;
            }


            anime.SetBool("Run", false);
        }


    }


    public void TakeDamage()
    {
        player.GetComponent<Player>().TakeDamage(Damage);
        anime.SetTrigger("Attack");


    }
}
