using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class GranadeDamage : MonoBehaviour
{


    public int Damage = 100;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(wait());
    }

    // Update is called once per frame
    void Update()
    {
        
    }




   

    private void OnTriggerEnter(Collider other)
    {


        if (other.CompareTag("Enemie"))
        {
            other.GetComponent<EnemiesHealth>().TakeDamage(Damage);
            other.GetComponent<CircleExplod>().TakeDamage(Damage);
        }
        if (other.CompareTag("Box"))
        {
            other.GetComponent<Box>().Break(Damage);
        }
        if (other.CompareTag("AmmoBox"))
        {
            other.GetComponent<AmmoCrate>().Break(Damage);
        }
        if (other.CompareTag("Health"))
        {
            other.GetComponent<HealthItem>().Break(Damage);
            Destroy(gameObject);
        }
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(1);

        Destroy(gameObject);
    }

}
