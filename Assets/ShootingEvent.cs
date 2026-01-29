using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEvent : MonoBehaviour
{
    public Gun gun;

    public EnemiesHealth enemieshealth;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void shoot()
    {


        gun.shoot();
    }


    public void takedamage()
    {
        enemieshealth.takedamage();
    }
}
