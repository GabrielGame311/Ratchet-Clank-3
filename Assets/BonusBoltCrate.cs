using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class BonusBoltCrate : MonoBehaviour
{



    public GameObject Effect;

    int box = 1;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnDestroy()
    {
        GameObject pl = Instantiate(Effect, transform.position, transform.rotation);
        Destroy(pl, 3);
        Bolts boltsScript = GameObject.FindObjectOfType<Bolts>();

        if (boltsScript != null)
        {
            // Dubbla multiplikatorn (1 -> 2 -> 4 -> 8...)
            boltsScript.scoreMultiplier *= 2;

            // Valfritt: Om du vill behålla din gamla bool också
            boltsScript.IsBonus = true;
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
        
    }

    public void Break(int damage)
    {
        box -= damage;


        if (box <= 0)
        {
            Destroy(gameObject);


        }


    }

}
