using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBall : MonoBehaviour
{
    public GameObject ball;
    public Transform spawn;
    public float BallSpeed;
    public  bool shoot = true;
   
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

        if (GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo > 0)
        {
            shoot = true;
        }
        else
        {
            shoot = false;
        }

        if (GetComponent<WeaponAmmos>().enabled)
        {
            if (IOSController.IosController_ == null)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    ThrowBall();
                }
            }

        }





    }



    public void ThrowBall()
    {
       
        


        if(shoot == true)
        {
            GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo  -= 1;
            if (GetComponent<WeaponAmmos>().Ammo < GetComponent<WeaponAmmos>().MaxAmmo)
            {
                GameObject.FindObjectOfType<VendingMenu>().Price += GetComponent<WeaponAmmos>().havePrice;
            }
            var banger = Instantiate(ball, spawn.transform.position, spawn.transform.rotation);
            banger.GetComponent<Rigidbody>().AddForce(spawn.transform.forward * BallSpeed);

            shoot = false;
        }
    }

    IEnumerator wait()
    {
        shoot = false;
        yield return new WaitForSeconds(3);

        shoot = true;
    }
}
