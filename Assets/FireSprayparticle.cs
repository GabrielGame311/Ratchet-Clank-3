using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSprayparticle : MonoBehaviour
{
    public int damage;
    bool damages = true;

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
        if(other.CompareTag("Player"))
        {
            if(damages == true)
            {
                other.GetComponent<Player>().TakeDamage(damage);

                StartCoroutine(wait());
            }
        }
    }

    IEnumerator wait()
    {
        damages = false;
        yield return new WaitForSeconds(2);
        damages = true;
    }
}
