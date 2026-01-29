using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigZombie : MonoBehaviour
{
    public Animator anime;

    public bool Attack = false;
    public GameObject bullet;
    public Transform spawn;
    public float bulletSpeed;
    private Transform target;
    public float speed;
   
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Attack == true)
        {
            transform.LookAt(target);

            anime.SetTrigger("Attack");



        }
        else if(Attack == false)
        {

        }


        if (EnemiesHealth.FindObjectOfType<EnemiesHealth>().GetComponent<EnemiesHealth>().health <= 0)
        {
            anime.SetTrigger("Dead");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Attack = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            //Attack = false;
        }
    }


   

    public void Dead()
    {
        
    }

    public void shoot()
    {

       
        var obj = Instantiate(bullet, spawn.transform.position, spawn.transform.rotation);
        obj.GetComponent<Rigidbody>().AddForce(spawn.transform.forward * bulletSpeed * Time.deltaTime);


    }

    
}
