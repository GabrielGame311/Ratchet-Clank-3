using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ObjectDamage : MonoBehaviour
{

    public int Damage;
    private VisualEffect effects;

    
   
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            other.GetComponent<Player>().TakeDamage(Damage);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Enemie")
        {

            other.GetComponent<EnemiesHealth>().TakeDamage(Damage);
        }

        if (other.tag == "Box")
        {
            Destroy(other.gameObject);
        }
        if (other.tag == "AmmoBox")
        {
            Destroy(other.gameObject);
        }
        if (other.tag == "Player")
        {

            other.GetComponent<Player>().TakeDamage(Damage);
        }
    }

    void TakeDamage()
    {
       
    }


}
