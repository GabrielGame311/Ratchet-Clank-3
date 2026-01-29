using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetHealth : MonoBehaviour
{

    public int health;

    public ParticleSystem particle;

    private GameObject player;

    public float speed;

    bool gethealth = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if(gethealth == true)
        {
            float step = speed * Time.deltaTime;

            particle.transform.position = Vector3.MoveTowards(particle.transform.position, player.transform.position, step);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<Player>().GetHealth(health);
            particle.Play();
            gethealth = true;
            StartCoroutine(wait());
        }

       

    }


    IEnumerator wait()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
