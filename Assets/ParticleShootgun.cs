using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleShootgun : MonoBehaviour
{

    public int Damage;

    public  bool takedamage = false;
    
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

                

                takedamage = false;
            }


            if (other.CompareTag("Box"))
            {


                
                
                    other.GetComponent<Box>().Break(Damage);

                    
                
            }

            if (other.CompareTag("AmmoBox"))
            {




                other.GetComponent<AmmoCrate>().Break(Damage);



            }

            if (other.CompareTag("Health"))
            {
                other.GetComponent<HealthItem>().Break(Damage);
               // Destroy(gameObject);
            }
        

       
    }



}
