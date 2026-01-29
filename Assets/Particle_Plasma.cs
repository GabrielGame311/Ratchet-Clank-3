using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle_Plasma : MonoBehaviour
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



    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemie"))
        {
            other.GetComponent<EnemiesHealth>().TakeDamage(Damage);
        }
        else if (other.CompareTag("Box"))
        {
            other.GetComponent<Box>().Break(Damage);
        }
        else if (other.CompareTag("AmmoBox"))
        {
            other.GetComponent<AmmoCrate>().Break(Damage);
        }
    }
}
