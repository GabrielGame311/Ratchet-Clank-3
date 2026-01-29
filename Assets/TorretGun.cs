using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorretGun : MonoBehaviour
{

    public GameObject ball;
    public GameObject torret;
    public GameObject Torretv5;
    public Transform ballpos;
    public Transform UISpawn;
   
    public Animator anime;

    public float BulletSpeed = 10;
    public int AmmoShoot;
    
    bool shoot = true;
   
    bool destroy = false;
    // Start is called before the first frame update
    void Start()
    {

        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<WeaponAmmos>().Ammo > 0)
        {


            shoot = true;
        }
        else
        {
            shoot = false;
        }

        if(GetComponent<WeaponAmmos>().enabled)
        {
            if (IOSController.IosController_ == null)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Fire();
                }
            }
        }

        
      
       

       

    }


    public void Fire()
    {
        if (shoot == true)
        {
            GetComponent<WeaponAmmos>().Ammo -= 1;

            anime.SetTrigger("Throw");
            if (GetComponent<WeaponAmmos>().Ammo < GetComponent<WeaponAmmos>().MaxAmmo)
            {
                GameObject.FindObjectOfType<VendingMenu>().Price += GetComponent<WeaponAmmos>().havePrice;
            }
        }

       
    }


    public void shoots()
    {
        anime.SetTrigger("Throw");
    }
    public void ThrowBalls()
    {

        
        

           


        if (GetComponent<WeaponsUI>().Level == 5)
        {
            var balls = GameObject.Instantiate(Torretv5, ballpos.transform.position, Quaternion.identity);
            balls.GetComponent<Rigidbody>().AddForce(ballpos.transform.forward * BulletSpeed);
            
            balls.GetComponent<Ball>().torret = Torretv5;


        }
        else
        {
            var balls = GameObject.Instantiate(ball, ballpos.transform.position, ballpos.transform.rotation);
            balls.GetComponent<Rigidbody>().AddForce(ballpos.transform.forward * BulletSpeed);
        }
 




    }

    
   


  
}
