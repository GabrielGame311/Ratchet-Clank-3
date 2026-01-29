using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VendingShop : MonoBehaviour
{
    public int Bolt;

    public TMP_Text Bolt_Text;
    public TMP_Text Price_Text;
    public int WeaponID;
    public AudioClip SoundShop;
    public AudioSource sound_;
    public GameObject ShopVendor_Button;
    public Image WeaponIcone;
    public TMP_Text WeaponAmmoText;
    public static VendingShop VendingShop_;
    public List<WeaponAmmos> weaponAmmosList = new List<WeaponAmmos>();
    public Transform Content_;
    bool buttonInstantiated = false;
    public int Price;
    public GameObject WeaponAmmoAll;

    // Start is called before the first frame update
    void Start()
    {
        sound_ = GetComponent<AudioSource>();
        VendingShop_ = GetComponent<VendingShop>();

        foreach (WeaponAmmos wp in Resources.FindObjectsOfTypeAll<WeaponAmmos>())
        {
            weaponAmmosList.Add(wp);
        }
    }




    // Update is called once per frame
    void Update()
    {

        GameObject.FindObjectOfType<VendingShop>().CheckWeaponAmmo();
        Bolt_Text.text = Bolt.ToString();
        Price_Text.text = "Cost:   " + Price; //GameObject.FindObjectOfType<WeaponAmmos>().havePrice.ToString();
       
        WeaponIcone.sprite = GameObject.FindObjectOfType<WeaponsUI>().WeaponImg;
        

        Bolt = GameObject.FindObjectOfType<Bolts>().bolt;
     


        
    }

    public void CheckWeaponAmmo()
    {



        foreach (WeaponAmmos wp in weaponAmmosList)
        {
            // Kontrollera om knappen redan har skapats
            if (wp.Ammo < wp.MaxAmmo && !wp.buttonInstantiated)
            {
               
               
                if(WeaponAmmoAll != null)
                {
                    WeaponAmmoAll.SetActive(true);
                }

                // Skapa en knapp för detta specifika vapen
                GameObject newButton = Instantiate(ShopVendor_Button, Content_);
               
                // Spara referensen till knappen så att den kan tas bort senare
                wp.buttonInstance = newButton;
                wp.buttonInstantiated = true;
                newButton.GetComponent<Shop_UI>().Icone.sprite = wp.GetComponent<WeaponsUI>().WeaponImg;
                // Anpassa ikonen och texten på knappen

                newButton.GetComponentInChildren<Text>().text = wp.name;

                newButton.SetActive(true);
            }
            // Om ammot är fullt och knappen redan har skapats
            else if (wp.Ammo >= wp.MaxAmmo && wp.buttonInstantiated)
            {
                // Förstör knappen och sätt flaggan till false
                if (wp.buttonInstance != null)
                {
                    Destroy(wp.buttonInstance);
                    wp.buttonInstance = null;
                }

                wp.buttonInstantiated = false;

                // Om `WeaponAmmoAll` är aktiv och det är dags att inaktivera den
                if (WeaponAmmoAll != null)
                {
                    WeaponAmmoAll.SetActive(false);
                }
            }
        }

    }



    public void Shop()
    {
       
        sound_.PlayOneShot(SoundShop);
        GameObject.FindObjectOfType<VendingMenu>().ShopAmmo();
    }
}
