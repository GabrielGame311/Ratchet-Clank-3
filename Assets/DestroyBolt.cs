using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBolt : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
