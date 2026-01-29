using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform bulletparticle;
    public GameObject bulletSpawnPoint;
    public float bulletspeed = 10;

    private Transform player;

    

    public float ShootTime;

    bool shooting = true;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
       





    }



   

    public void shoot()
    {

       
       
         
        
            
            
            





                var bullet = Instantiate(bulletparticle, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
                bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.transform.forward * bulletspeed;

                
            







        
    }

    IEnumerator ShootingDelay()
    {

        shooting = false;
        yield return new WaitForSeconds(ShootTime);

        shooting = true;
        

       

    }


}
