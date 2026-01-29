using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeWeapons : MonoBehaviour
{
    public Transform[] Weapons;
    public int WeaponSelect = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        WeaponSelect = GameObject.FindObjectOfType<WeaponSwitcher>().WeaponSelecter;


        int Selected = WeaponSelect;
        int i = 0;

        foreach (Transform weapon in Weapons)
        {

            if (i == WeaponSelect)
            {
                weapon.gameObject.SetActive(true);

            }
            else
            {
                weapon.gameObject.SetActive(false);

            }

            i++;
        }
    }
}
