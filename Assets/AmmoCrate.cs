using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoCrate : MonoBehaviour
{

    int box = 1;
    int maxbox;

    public Transform Cracked;
    private GameObject obj;
    public Transform Ammo;
    bool down = false;
    public GameObject spawn;
    // Start is called before the first frame update
    void Start()
    {
        box = maxbox;
        obj = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Instantiate(Cracked, transform.position, transform.rotation);
        Instantiate(Ammo, spawn.transform.position, transform.rotation);
    }

    public void Break(int damage)
    {
        box -= damage;


        if (box <= 0)
        {
            Destroy(gameObject);
           

        }


    }

   

   


    private void OnCollisionExit(Collision other)
    {
        if (other.collider.tag == "Player")
        {
            Rigidbody otherRigidbody = other.collider.GetComponent<Rigidbody>();


            otherRigidbody.isKinematic = false;

        }
    }

    private void OnCollisionEnter(Collision other)
    {
       
            StartCoroutine(wait());

        

        
    }


    IEnumerator wait()
    {

        yield return new WaitForSeconds(1);
        GetComponent<Rigidbody>().isKinematic = true;
        down = true;
    }
}
