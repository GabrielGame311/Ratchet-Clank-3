using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBullet : MonoBehaviour
{

    public float Speed;

    public float FollowTime;
    public float SpeedRotate;
    Transform player;
    public float homingDistance = 5f;

    bool IsDistance = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {


            GetComponentInChildren<MeshRenderer>().transform.Rotate(+SpeedRotate, 0, 0);


            FollowTime -= Time.deltaTime;


            if(FollowTime < 0)
            {

                Destroy(gameObject);
            }
            if(FollowTime < 19)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;

                if (Vector3.Distance(transform.position, player.position) > homingDistance)
                {
                    // If the distance is greater than the homing distance, move forward

                    if(IsDistance == false)
                    {
                        transform.LookAt(player);
                        GetComponent<Rigidbody>().velocity = directionToPlayer * Speed;
                    }
                   
                }
                else
                {
                    // If the distance is less than or equal to the homing distance, start homing


                    IsDistance = true;
                    GetComponent<Rigidbody>().velocity = transform.forward * Speed;
                }
            }
        }
    }
}
