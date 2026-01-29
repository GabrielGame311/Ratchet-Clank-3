using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDamage : MonoBehaviour
{
    
    
    public int Damage;

    public GameObject explode;
   
   

    private void Update()
    {
    }


   
    private void OnParticleCollision(GameObject other)
    {
        Destroy(gameObject);
       
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().TakeDamage(Damage);
            
        }

        if(other.tag == "Enemie")
        {
            other.GetComponent<EnemiesHealth>().TakeDamage(Damage);
        }
        if(other.tag == "Box")
        {
            Destroy(other.gameObject);
        }
        if (other.tag == "AmmoBox")
        {
            Destroy(other.gameObject);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {

       

        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<Player>().TakeDamage(Damage);
           
        }
        if (collision.collider.tag == "Enemie")
        {
            collision.collider.GetComponent<EnemiesHealth>().TakeDamage(Damage);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
       GameObject prefab = Instantiate(explode, transform.position, transform.rotation);
        Destroy(prefab, 2);
    }

}
