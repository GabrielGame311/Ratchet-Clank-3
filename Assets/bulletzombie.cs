using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletzombie : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {

        if (other.collider.CompareTag("Player"))
        {
            other.collider.GetComponent<Player>().TakeDamage(20);
            Destroy(gameObject);
        }
        if (other.collider.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Player>().TakeDamage(20);
            Destroy(gameObject);
        }
        if(other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
