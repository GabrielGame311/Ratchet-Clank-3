using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBall : MonoBehaviour
{
    public GameObject ball;
    public Transform spawn;
    public float BallSpeed;
    public bool shoot = true;
    private WeaponAmmos weaponAmmo;

    // Start is called before the first frame update
    void Start()
    {
        weaponAmmo = GetComponent<WeaponAmmos>();
    }

    // Update is called once per frame
    void Update()
    {

        if (weaponAmmo != null && weaponAmmo.Ammo > 0)
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
        if (shoot == true && weaponAmmo != null && weaponAmmo.TryShoot())
        {
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
