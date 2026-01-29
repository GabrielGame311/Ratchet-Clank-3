using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthItem : MonoBehaviour
{

    int health = 1;
    int maxhealth;

    public GameObject healthparticle;
    public GameObject crackedobj;
    

    public GameObject spawn;
    // Start is called before the first frame update
    void Start()
    {
        health = maxhealth;
       
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void Break(int damage)
    {
       health -= damage;


        if (health <= 0)
        {
            Destroy(gameObject);
           // Instantiate(Cracked, transform.position, transform.rotation);
            Instantiate(healthparticle, spawn.transform.position, transform.rotation);

        }


    }
}
