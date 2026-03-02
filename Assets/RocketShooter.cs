using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketShooter : MonoBehaviour
{
    public Transform ShootPoint;
    public float ShootSpeed;
    public GameObject BulletPrefab;

    public float ShootTime;

    public float TimeToShoot;
    public bool IsShooting = true;
    public int followTime = 3;
    float StartShoot;
    List<GameObject> bullets = new List<GameObject>();
    public int PlayerShoot;
    GameObject player;
    Quaternion rot;
    public GameObject Rangers;

    public bool RangerShoot = false;
         

    // Start is called before the first frame update
    void Start()
    {
        StartShoot = ShootTime;

        player = GameObject.FindGameObjectWithTag("Player");

        rot = ShootPoint.transform.rotation;

    }

    // Update is called once per frame
    void Update()
    {
        
        if(RangerShoot)
        {
            foreach (GameObject bullet in bullets)
            {
                bullet.transform.position = Vector3.MoveTowards(bullet.transform.position, Rangers.transform.position, ShootSpeed * Time.deltaTime);
            }
        }
        

        if (PlayerShoot > 4)
        {
            
        } 
        if(PlayerShoot == 8)
        {
            PlayerShoot = 0;
        }


            float startimeshoot = 0;

        if(TimeToShoot > startimeshoot)
        {
            TimeToShoot -= Time.deltaTime;
            ShootTime -= Time.deltaTime;
        }

        if(ShootTime < 0)
        {

            Shoot();
            ShootTime = StartShoot;
            
        }


    }
  

    void Shoot()
    {
        GameObject bulletPrefab = Instantiate(BulletPrefab, ShootPoint.transform.position, ShootPoint.transform.rotation);
      
        bullets.Add(bulletPrefab);
        bulletPrefab.GetComponent<Rigidbody>().linearVelocity = ShootPoint.transform.forward * ShootSpeed;








        PlayerShoot++;


        Destroy(bulletPrefab, 50);
    }

  

}
