using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponAmmoCount : MonoBehaviour
{

    public TMP_Text Ammo_Text;
    public Image weaponIcone;
    public static WeaponAmmoCount WeaponAmmoCount_;
    // Start is called before the first frame update
    void Start()
    {
        WeaponAmmoCount_ = GetComponent<WeaponAmmoCount>();



    }

    // Update is called once per frame
    void Update()
    {



        int currentAmmo = GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo;
        int maxAmmo = GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().MaxAmmo;

        string ammoText = currentAmmo.ToString("D2"); // Format to two digits
        string maxAmmoText = maxAmmo.ToString("D2"); // Format to two digits

        Ammo_Text.text = ammoText + "/" + maxAmmoText;
        VendingShop.VendingShop_.WeaponAmmoText.text = ammoText + "/" + maxAmmoText;














    }
}
