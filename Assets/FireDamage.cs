using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDamage : MonoBehaviour
{

    public int Damage = 15;
    GameObject player;
    float fireTime;
    public FirePlane fireplane_;
    public BoxCollider Collider;
    public FirePlay fireplay_;
    public bool IsTrigger = false;

    public bool IsFire = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        Collider = GetComponent<BoxCollider>();
        fireTime = fireplane_.FireTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsFire == false)
        {
            if (2 < fireplane_.FireTime)
            {
                Collider.enabled = true;
            }
            else
            {
                Collider.enabled = false;
                IsTrigger = false;
            }

        }

    }


    private void OnParticleCollision(GameObject other)
    {
        if(other.tag == "Player")
        {
           
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            
            
            
                if (IsTrigger == false)
                {
                    player.GetComponent<Player>().TakeDamage(Damage);
                    IsTrigger = true;
                }
            
           

          
        }
    }
}
