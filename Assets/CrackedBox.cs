using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrackedBox : MonoBehaviour
{
    // Start is called before the first frame update


    GameObject[] crackedobj;


    void Start()
    {

        crackedobj = GetComponentsInChildren<GameObject>();

        foreach (GameObject obj in crackedobj)
        {
            Destroy(obj, 2);
        }
            Destroy(gameObject, 2);
    }

    // Update is called once per frame
   

   


    private void OnCollisionEnter(Collision collision)
    {

       

        if(collision.collider)
        {
            foreach (GameObject obj in crackedobj)
            {
                obj.GetComponent<Rigidbody>().useGravity = false;
                obj.GetComponent<MeshCollider>().isTrigger = true;
            }
        }
    }

   
}
