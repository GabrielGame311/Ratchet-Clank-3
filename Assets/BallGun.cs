using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGun : MonoBehaviour
{
    public GameObject littleBall;
    public int Damage;
    
   
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Instantiate(littleBall, transform.position, transform.rotation);
        GameObject.FindObjectOfType<GunBall>().shoot = true;
    }

    private void OnCollisionEnter(Collision other)
    {

        if (other.collider.CompareTag("Enemie"))
        {
            other.collider.GetComponent<CircleExplod>().TakeDamage(Damage);
           
        }

        if (other.collider.CompareTag("Box"))
        {
            other.collider.GetComponent<Box>().Break(Damage);
            Destroy(gameObject);
        }
        if(other.collider.CompareTag("AmmoBox"))
        {
            other.collider.GetComponent<AmmoCrate>().Break(Damage);
            Destroy(gameObject);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemie"))
        {
            other.GetComponent<EnemiesHealth>().TakeDamage(Damage);
            Destroy(gameObject);
        }
    }

  
}
