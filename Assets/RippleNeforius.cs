using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class RippleNeforius : MonoBehaviour
{

    public int Damage;
    public BoxCollider coll;
    public float ScaleTime;
    // Start is called before the first frame update
    void Start()
    {
       coll = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }


    private void OnParticleCollision(GameObject other)
    {
        if(other.tag == "Player")
        {
            other.GetComponent<Player>().TakeDamage(Damage);
            Debug.Log("DamageCollision");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<Player>().TakeDamage(Damage);
            Debug.Log("DamageTrigger");
        }
    }

    private void OnParticleTrigger()
    {


       
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        GetComponent<BoxCollider>().isTrigger = true;
        GetComponent<Rigidbody>().useGravity = false;
    }


}
