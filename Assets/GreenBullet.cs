using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenBullet : MonoBehaviour
{
    public float MoveSpeed;

   
    public AudioClip PickupSound;
    public GameObject GreenEffect;

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

            MoveSpeed = 30;
            GreenEffect.SetActive(true);
            GameObject.FindObjectOfType<HackerGameUI>().HasHit();
            GetComponent<MeshCollider>().isTrigger = true;
            GameObject.FindObjectOfType<ShootGame>().Sound.PlayOneShot(PickupSound);
            Destroy(collision.collider.gameObject);

        }
        else
        {
            Destroy(gameObject);
        }
        

       
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Point")
        {
            Destroy(gameObject);
        }
    }
}
