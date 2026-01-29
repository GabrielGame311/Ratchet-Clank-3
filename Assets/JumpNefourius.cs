using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpNefourius : MonoBehaviour
{
    public WeaponNefrouis wp;
    public Nefourius Nefourius_;

    // Start is called before the first frame update
    void Start()
    {
       // Nefourius_ = GetComponentInChildren<Nefourius>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void JumpLeft()
    {
        Nefourius_.transform.LookAt(Nefourius_.player.transform);
        Nefourius_.JumpLeft();
    }
    public void JumpRight()
    {
        Nefourius_.transform.LookAt(Nefourius_.player.transform);
        Nefourius_.JumpRight();
    }


    public void ShootGranade()
    {
        Nefourius_.ShootGranade();
    }

    public void Shoot()
    {
        Nefourius_.Shoot();
    }

    public void ShootWeapon()
    {
        wp.ISActive = true;
    }
    public void ShootWeaponFalse()
    {
        wp.ISActive = false;
    }
}
