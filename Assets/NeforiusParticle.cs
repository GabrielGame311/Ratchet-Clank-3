using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeforiusParticle : MonoBehaviour
{

    public GameObject Particle;
    public GameObject destoryobj;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnDestroy()
    {

        GameObject pr = Instantiate(Particle, transform.position, Quaternion.Euler(90f, 0f, 0f));

        Destroy(pr, 5);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(destoryobj);
    }


    private void OnParticleCollision(GameObject other)
    {
        Destroy(gameObject);
    }

}
