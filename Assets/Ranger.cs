using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ranger : MonoBehaviour
{
    public Animator anime;
    private NavMeshAgent myAgnet;
    public Transform target;
    public Transform distancenemies;
    public bool chaseTarget = true;
    public float stopingDistance = 2.5f;
    public float delayBeetweenAttacks = 1.5f;
    private float attackCooldown;

    private float distandeFromTarget;




    public int damage = 50;



    // Start is called before the first frame update
    void Start()
    {

        myAgnet = GetComponent<NavMeshAgent>();
        myAgnet.stoppingDistance = stopingDistance;
        attackCooldown = Time.time;




    }


    // Update is called once per frame
    void Update()
    {


        ChaseTarget();




    }






    void ChaseTarget()
    {





        distandeFromTarget = Vector3.Distance(target.position, transform.position);
        if (distandeFromTarget >= stopingDistance)
        {

            chaseTarget = true;
        }
        else
        {



            chaseTarget = false;
            Attack();

        }

        if (chaseTarget)
        {
            myAgnet.SetDestination(target.position);

            anime.SetBool("Walk", true);

            Vector3 toPlayer = target.transform.position - transform.position;
            if (Vector3.Distance(distancenemies.transform.position, -transform.position) < 3)
            {
                Vector3 targetPosition = toPlayer.normalized * -3f;
                myAgnet.destination = targetPosition;
                myAgnet.Resume();


            }

        }
        else
        {

        }




        void Attack()
        {
            if (Time.time > attackCooldown)
            {

                attackCooldown = Time.time + delayBeetweenAttacks;
                anime.SetBool("Shoot", true);

                NavMeshHit hit;
                if (myAgnet.FindClosestEdge(out hit))
                {
                    myAgnet.SetDestination(hit.position);
                }

            }


        }
    }

}
