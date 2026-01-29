using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGetHealth : MonoBehaviour
{

    public float speed;
    private GameObject player;
    bool gethealth = false;
    public int GetHealth;
    private Rigidbody rb;
    public GameObject destoryed;
    private ParticleSystem particle;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Bolt");
        particle = GetComponent<ParticleSystem>();

    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().currentHealth < GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().maxHealth)
        {
            if (gethealth == true)
            {
                float step = speed * Time.deltaTime;
                particle.Play();
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);
            }
        }


       
    }

    private void OnDestroy()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().currentHealth += GetHealth;
        Destroy(destoryed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            gethealth = true;
           
           
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if(other.CompareTag("Player"))
        {
            if (gethealth == true)
            {
                if (GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().currentHealth < GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().maxHealth)
                {

                    Destroy(gameObject);
                }

                
            }


                
        }
    }
}
