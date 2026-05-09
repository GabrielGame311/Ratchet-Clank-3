using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class SwampMonster : MonoBehaviour
{
    public float PrefabBolSpeed;
    public bool IsAttack;

    public float AttackTime;
    float startAttackTime;

    public GameObject PrefabAttack;

    public Transform PrefabPoint;
    GameObject Player_;
    public float PlayerDistance;
    Animator anime;

    public AudioSource sound;
    public AudioClip[] Soundfx;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sound = GetComponent<AudioSource>();
        anime = GetComponent<Animator>();
        Player_ = GameObject.FindGameObjectWithTag("Player");
        startAttackTime = AttackTime;

    }

    // Update is called once per frame
    void Update()
    {

        float dis = Vector3.Distance(transform.position, Player_.transform.position);

        if(dis < PlayerDistance)
        {
            anime.SetTrigger("Start");

            IsAttack = true;

        }
        else
        {
            IsAttack = false;

        }


        if(IsAttack)
        {

            AttackTime -= Time.deltaTime;


            if(AttackTime < 0)
            {


                AttackTime = startAttackTime;
                anime.SetTrigger("Attack");
            }


        }
        else
        {




        }

    }


    public void Attack()
    {

        GameObject prefab = Instantiate(PrefabAttack, PrefabPoint.transform.position, PrefabPoint.transform.rotation);

        sound.PlayOneShot(Soundfx[1]);

        foreach (Rigidbody pr in prefab.GetComponentsInChildren<Rigidbody>())
        {
            pr.linearVelocity = PrefabPoint.transform.forward * PrefabBolSpeed;
        }

       
    }


    public void Die()
    {

        
        sound.clip = Soundfx[2];

        sound.Play();
    }

    public void Startanime()
    {

        sound.clip = Soundfx[0];

        sound.Play();
    }
}
