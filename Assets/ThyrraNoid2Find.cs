using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThyrraNoid2Find : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Glad()
    {
        GetComponentInParent<ThyrraNoid2>().Glad();

    }

    public void Shoot()
    {
        GetComponentInParent<ThyrraNoid2>().Shoot();
    }

    public void ShootParticle()
    {
        GetComponentInParent<ThyrraNoid2>().ShootParticle();
    }
}
