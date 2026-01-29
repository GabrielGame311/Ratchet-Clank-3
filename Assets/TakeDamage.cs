using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamage : MonoBehaviour
{



    public float Damage;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        if(collision.collider.tag == "Ranger")
        {
            collision.collider.gameObject.GetComponent<RangerHealth>().TakeDamage(Damage);
        }
        if(collision.collider.tag == "Player")
        {
            collision.collider.GetComponent<PlayerShip>().TakeDamage(((int)Damage));
        }
    }


}
