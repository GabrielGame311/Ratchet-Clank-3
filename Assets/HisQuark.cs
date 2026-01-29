using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HisQuark : MonoBehaviour
{

    public float GoUpSpeed;
    public Transform posPoint;

    Vector3 pos;
    public bool IsTrigger = false;
    GameObject Player;
    // Start is called before the first frame update
    void Start()
    {
        pos = transform.position;
    }

    private void LateUpdate()
    {
        if(IsTrigger)
        {
            transform.position = Vector3.MoveTowards(transform.position, posPoint.transform.position, GoUpSpeed * Time.deltaTime);
            transform.LookAt(posPoint);

            if(transform.position == posPoint.transform.position)
            {
                IsTrigger = false;
                Player = null;

            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the elevator.");
            IsTrigger = true;
            Player = other.gameObject;
            other.transform.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited the elevator.");
            IsTrigger = false;
            Player = null;
            other.transform.parent = null;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Player entered the elevator.");
            IsTrigger = true;
            Player = collision.collider.gameObject;
            collision.collider.transform.parent = transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Player exited the elevator.");
            IsTrigger = false;
            Player = null;
            collision.collider.transform.parent = null;
        }
    }

}
