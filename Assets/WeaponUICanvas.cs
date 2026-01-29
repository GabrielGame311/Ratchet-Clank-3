using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class WeaponUICanvas : MonoBehaviour
{



    public List<Image> images;
    public List<WeaponsUI> img;
    public int WeaponSelect = 0;
    public int weapon;
    public static WeaponUICanvas weaponcanvas_;
    

    // Start is called before the first frame update
    void Start()
    {
        //WeaponsUI[] img = FindObjectsOfType<WeaponsUI>();
        // img.Add(GameObject.FindObjectOfType<WeaponsUI>());

        weaponcanvas_ = GetComponent<WeaponUICanvas>();
       


    }

  


    // Update is called once per frame
    void Update()
    {
       
       
      


        if (WeaponSelect == 0)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = 0;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
        }
        if (WeaponSelect == 1)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = 1;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
        }
        if (WeaponSelect == 2)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = 2;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
        }
        if (WeaponSelect == 3)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = 3;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
        }
        if (WeaponSelect == 4)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = 4;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
        }
        if (WeaponSelect == 5)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = 5;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
        }
        if (WeaponSelect == 6)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = 6;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
        }
        if (WeaponSelect == 7)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = 7;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();

        }


       


    }

    public void UpdateWeaponImage(Sprite newSprite, int weaponID)
    {
       
        
        
        // Update the weapon image for the specified weapon ID.
        images[weaponID].sprite = newSprite;
    }


    public void SwitchWeapon(int wp)
    {
        WeaponSelect = wp;



    }


}

