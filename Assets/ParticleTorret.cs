using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTorret : MonoBehaviour
{


   
    private string enemie;

    public int damage;

    public static ParticleTorret particletorret_;
    public ParticleSystem particle;


  

    private void Start()
    {
        particletorret_ = GetComponent<ParticleTorret>();
        particle = GetComponent<ParticleSystem>();
    }


    private void Update()
    {
        
    }

    void OnParticleCollision(GameObject other)
    {




        Destroy(gameObject);
     

        if(other.CompareTag("Enemie"))
        {
            other.GetComponent<EnemiesHealth>().TakeDamage(damage);
        }

        if (other.CompareTag("Box"))
        {




            other.GetComponent<Box>().Break(damage);



        }
        if (other.CompareTag("AmmoBox"))
        {
            other.GetComponent<AmmoCrate>().Break(damage);
        }

        if (other.CompareTag("Health"))
        {
            other.GetComponent<HealthItem>().Break(damage);
            Destroy(gameObject);
        }

    }



  


    public void Fire()
    {
       // particle.Play();
       
       
    }

}
