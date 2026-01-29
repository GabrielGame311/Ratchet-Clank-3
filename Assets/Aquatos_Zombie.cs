using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Aquatos_Zombie : MonoBehaviour
{

    public Animator anime;
    public GameObject Enemie;
    private Transform player;

    private float mindis;

    public int speed;

    public int Stoppdistance;

    public int Damage;
    public float DamageTime;

    bool FollowPlayer = false;

    bool damage = false;

    private AudioSource sound;
    public AudioClip SoundAttack;

    private SphereCollider collidier;
    private WrenchEnemies Wrench;
    public GameObject minizombie;
    Vector3 startpos;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        sound = GetComponent<AudioSource>();

        collidier = GetComponent<SphereCollider>();

        Wrench = GameObject.FindObjectOfType<WrenchEnemies>();

        startpos = transform.position;

        
        
    }

    private void OnDestroy()
    {
        Instantiate(minizombie, transform.position, transform.rotation);
    }

    void Update()
    {

        


        float dist = Vector3.Distance(transform.position, player.transform.position);


       
           if(FollowPlayer == true)
           {
                transform.LookAt(player);


                
                if(dist <= Stoppdistance)
                {
                    anime.SetTrigger("Chasse");
                    anime.SetBool("Walk", false);
                    

            }
                else
                {
                    if (Vector3.Distance(transform.position, player.transform.position) >= mindis)
                    {
                        transform.position += transform.forward * speed * Time.deltaTime;
                        
                        anime.SetBool("Walk", true);
                        
                         
                        
                    }
                }

                   
                
           }
       
          

        
       
    }




    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            FollowPlayer = true;
            damage = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            damage = false;
        }
    }

    public void damages()
    {
        player.GetComponent<Player>().TakeDamage(Damage);
        sound.PlayOneShot(SoundAttack);
    }
    IEnumerator Damagewait()
    {
        damage = false;
        yield return new WaitForSeconds(DamageTime);
        sound.PlayOneShot(SoundAttack);
        player.GetComponent<Player>().TakeDamage(Damage);

        damage = true;

    }



}
