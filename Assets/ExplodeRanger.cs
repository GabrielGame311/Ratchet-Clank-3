using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeRanger : MonoBehaviour
{
    public float radius = 5;
    public int Damage;
    public float force = 700;
    public float jumpForce = 5f;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius);
    }

}
