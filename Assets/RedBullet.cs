using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBullet : MonoBehaviour
{


    public float MoveSpeed;
    
    public AudioClip ExplodeSound;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if(collision.collider.tag == "GreenShoot")
        {


        }
        else
        {
            GameObject.FindObjectOfType<ShootGame>().Sound.PlayOneShot(ExplodeSound);
            Destroy(gameObject);
        }
           
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Point")
        {
            Destroy(gameObject);
        }
    }
}
