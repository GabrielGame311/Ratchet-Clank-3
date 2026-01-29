using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunShooter : MonoBehaviour
{
    public GameObject particle;

    public Transform ProjectileSpawn;
    public Transform Projectile2Spawn;

    int shootcount = 0;

    float shootTime = 0.2f;
    float startShoot;
    public float ShootSpeed;
    bool Isshoot = false;
    PlayerControlls controlls;
    bool shoots = true;
    private void Start()
    {

        startShoot = shootTime;

       
    }

    private void Awake()
    {
        controlls = new PlayerControlls();
    }

    private void OnEnable()
    {
        controlls.Enable();
    }

    private void OnDisable()
    {
        controlls.Disable();
    }

    // Update is called once per frame
    void Update()
    {


        if (GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo > 0)
        {


            shoots = true;
        }
        else
        {
            shoots = false;
        }

        if(shootTime < 0)
        {





            Isshoot = false;

            shootTime = startShoot;
        }

        if(Isshoot)
        {
            shootTime -= Time.deltaTime;
           
        }

        if(Isshoot == false)
        {
            if (GetComponent<WeaponAmmos>().enabled)
            {
                if (IOSController.IosController_ == null)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {

                        Isshoot = true;
                        shoot();
                    }
                }
            }
        }
      


    }

    public void shoot()
    {


        if (shoots == true)
        {
            
               GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo -= 1;

          
            if(SightUI.SightUI_.Sight.activeSelf)
            {
              
                if(shootcount == 0)
                {
                    Vector3 targetPosition = Camera.main.ScreenToWorldPoint(SightUI.SightUI_.Sight.transform.position);
                    Vector3 direction = (targetPosition - ProjectileSpawn.position).normalized;


                    GameObject bullet = Instantiate(particle, ProjectileSpawn.position, ProjectileSpawn.transform.rotation);
                    Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
                    bulletRigidbody.velocity = direction * ShootSpeed;

                    Quaternion rotation = Quaternion.LookRotation(direction);
                    bullet.transform.rotation = rotation;
                    shootcount = 1;
                }
                else if(shootcount == 1)
                {

                    Vector3 targetPosition2 = Camera.main.ScreenToWorldPoint(SightUI.SightUI_.Sight.transform.position);
                    Vector3 direction2 = (targetPosition2 - Projectile2Spawn.position).normalized;


                    GameObject bullet2 = Instantiate(particle, Projectile2Spawn.position, Projectile2Spawn.transform.rotation);
                    Rigidbody bulletRigidbody2 = bullet2.GetComponent<Rigidbody>();
                    bulletRigidbody2.velocity = direction2 * ShootSpeed;

                    Quaternion rotation2 = Quaternion.LookRotation(direction2);
                    bullet2.transform.rotation = rotation2;
                    shootcount = 0;
                }


               

               


            }
            else
            {


                if (shootcount == 0)
                {

                    var projectile = Instantiate(particle, ProjectileSpawn.transform.position, ProjectileSpawn.transform.rotation);
                    projectile.GetComponent<Rigidbody>().velocity = ProjectileSpawn.transform.forward * ShootSpeed;
                    projectile.transform.localScale = ProjectileSpawn.transform.localScale;
                   
                    shootcount = 1;
                }
                else if (shootcount == 1)
                {

                    var projectile2 = Instantiate(particle, Projectile2Spawn.transform.position, Projectile2Spawn.transform.rotation);
                    projectile2.GetComponent<Rigidbody>().velocity = Projectile2Spawn.transform.forward * ShootSpeed;
                    projectile2.transform.localScale = Projectile2Spawn.transform.localScale;
                    shootcount = 0;
                }
            }






            if (GetComponent<WeaponAmmos>().Ammo < GetComponent<WeaponAmmos>().MaxAmmo)
            {
                GameObject.FindObjectOfType<VendingMenu>().Price += GetComponent<WeaponAmmos>().havePrice;
            }
        }

       



        
    }

    


}
