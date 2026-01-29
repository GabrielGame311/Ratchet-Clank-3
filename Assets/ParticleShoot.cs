using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleShoot : MonoBehaviour
{

    public int Damage;

    private GameObject player;

   

   

    // Start is called before the first frame update
    void Start()
    {
       
      
        player = GameObject.FindGameObjectWithTag("Player");

        
    }

   

    private void OnParticleCollision(GameObject other)
    {


        Destroy(gameObject);

        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().TakeDamage(Damage);

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
       


        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().TakeDamage(Damage);

            Destroy(gameObject);
        }
    }
}
