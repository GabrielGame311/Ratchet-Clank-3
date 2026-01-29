using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb_box : MonoBehaviour
{
    public int Damage;
    public ParticleSystem particle;
    public ParticleSystem particle1;
    public ParticleSystem particle2;

    public AudioSource sound;
    public GameObject player;

    public List<GameObject> box = new List<GameObject>();

    
    Animator anime;
    bool boom = false;

    public Animator playeranime;

    public Animator player2;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        player2 = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        playeranime = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        anime = GetComponent<Animator>();

        
       
    }


    private void Update()
    {
        

       

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {






            StartCoroutine(wait());







        }

        if (other.CompareTag("Box"))
        {



            box.Add(other.gameObject);












        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {






            StartCoroutine(wait());







        }

        if (collision.collider.CompareTag("Box"))
        {



            box.Add(collision.collider.gameObject);












        }
    }




    IEnumerator wait()
    {
        
        anime.SetTrigger("Boom");
        
        sound.Play();
        yield return new WaitForSeconds(3.6f);
        particle.Play();
        particle1.Play();
        particle2.Play();
        boom = true;
        if (boom == true)
        {


            

            for(int i = 0; i < box.Count; i++)
            {
                box[i].gameObject.GetComponent<Box>().Break(2);
            }
            Debug.Log("BOOM!!");





            player.GetComponent<Player>().TakeDamage(Damage);

           
            boom = false;

            
        }

        yield return new WaitForSeconds(1);
        Destroy(gameObject);

    }







}
