using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerHealth : MonoBehaviour
{

    public float Health;
    public float MaxHealth;
    public GameObject CrackedRanger;

    public bool Explode = false;
  
    public float ExplodeTime;
    
    // Start is called before the first frame update
    void Start()
    {
        MaxHealth = Health;
    }

    // Update is called once per frame
    void Update()
    {
        

        if(Explode)
        {


           


                Destroy(gameObject, ExplodeTime);
            

        }
    }

    private void OnDestroy()
    {
        GameObject cracked = Instantiate(CrackedRanger, transform.position, transform.rotation);
        
        Destroy(cracked, 4);
    }

    public void TakeDamage(float damage)
    {

        Health -= damage;

        if(Health < 0)
        {
            Health = 0;
            Die();

        }



    }


    void Die()
    {


        Destroy(gameObject, 2);
    }
}
