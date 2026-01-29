using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootGameBullet : MonoBehaviour
{

    public float GreenBulletSpeed;



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

        if(collision.collider.tag == "Bullet")
        {
            
            Destroy(collision.collider.gameObject);
        }
        if (collision.collider.tag == "GreenBullet")
        {

            Destroy(collision.collider.gameObject);
        }


        Destroy(gameObject);
    }
}
