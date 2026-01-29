using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LittleBall : MonoBehaviour
{
    public float radius = 5;
    public int Damage;
    public float force = 700;
    public float jumpForce = 5f;


    public LayerMask Layer_;

    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius);
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnCollisionEnter(Collision other)
    {
        if (((1 << other.gameObject.layer) & Layer_) != 0)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * jumpForce * Random.Range(0.8f, 1.2f), ForceMode.Impulse);
        }
        

        Destroy(gameObject, 10);
       
        if (other.collider.CompareTag("Box"))
        {
            other.collider.GetComponent<Box>().Break(Damage);
            
        }
        if (other.collider.CompareTag("AmmoBox"))
        {
            other.collider.GetComponent<AmmoCrate>().Break(Damage);
            
        }
        if (other.collider.CompareTag("Health"))
        {
            other.collider.GetComponent<HealthItem>().Break(Damage);
            
        }
        if (other.collider.CompareTag("Enemie"))
        {
            
            if(other.collider.gameObject.GetComponent<EnemiesHealth>() != null)
            {
                other.collider.gameObject.GetComponent<EnemiesHealth>().TakeDamage(Damage);
                Destroy(gameObject);
            } 
            

            if (other.collider.GetComponent<CircleExplod>() != null)
            {
                other.collider.GetComponent<CircleExplod>().TakeDamage(Damage);
            }


        }
       

    }


  
   

}
