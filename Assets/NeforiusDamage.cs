using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeforiusDamage : MonoBehaviour
{

    public int Damage;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.gameObject.GetComponent<Player>().TakeDamage(Damage);
        }
        if (other.tag == "AmmoBox")
        {
            other.GetComponent<AmmoCrate>().Break(Damage);
        }

    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<Player>().TakeDamage(Damage);
        }
        if(other.tag == "AmmoBox")
        {
            other.GetComponent<AmmoCrate>().Break(Damage);
        }

        Destroy(gameObject);
    }


    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
