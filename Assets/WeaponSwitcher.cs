using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class WeaponSwitcher : MonoBehaviour
{
    
    public List<Transform> Weapons = new List<Transform>();
    public int WeaponSelecter = 0;

    public static WeaponSwitcher WeaponSwitcher_;
    public List<WeaponsUI> WeaponsUI_ = new List<WeaponsUI>();
    public Transform Wrench_;

    public Transform HackerItem;

    public Transform Hypershot_;


    private bool isInitialized = false;
    // Start is called before the first frame update
    void Start()
    {
        WeaponSwitcher_ = GetComponent<WeaponSwitcher>();




       



        GameObject handObject = gameObject;

        if (handObject != null)
        {
            // Hämta alla WeaponsUI-komponenter som är kopplade till "Hand" och dess barn
            WeaponsUI[] weaponsUIComponents = handObject.GetComponentsInChildren<WeaponsUI>(true);
            
            // Lägg till dessa komponenter till WeaponsUI_-listan
            WeaponsUI_.AddRange(weaponsUIComponents);
            foreach (WeaponsUI weaponsUI in WeaponsUI_)
            {
                if (weaponsUI != null)
                {
                    Weapons.Add(weaponsUI.transform);
                }
            }


        }
      


        WeaponSwitch();

        UpdateWeaponUI();
    }



    void UpdateWeaponUI()
    {
        // Hämta WeaponUICanvas-objektet
        WeaponUICanvas weaponUICanvas = GameObject.FindObjectOfType<WeaponUICanvas>();

        if (weaponUICanvas == null)
        {
            Debug.LogError("WeaponUICanvas not found!");
            return;
        }

        // Töm UI-bilderna
        foreach (var image in weaponUICanvas.images)
        {
           // image.enabled = false;
        }

        // Uppdatera UI-bilder baserat på vapnens ordning
        for (int i = 0; i < WeaponsUI_.Count; i++)
        {
            WeaponsUI wp = WeaponsUI_[i];

            if (wp.WeaponImg != null && i < weaponUICanvas.images.Count)
            {
                // Aktivera och uppdatera UI-bilden för vapnet
                weaponUICanvas.img.Add(wp);
                weaponUICanvas.images[i].enabled = true;
                weaponUICanvas.images[i].sprite = wp.WeaponImg;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {


       


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            WeaponSelecter = 0;
            WeaponSwitch();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            WeaponSelecter = 1;
            WeaponSwitch();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            WeaponSelecter = 2;
            WeaponSwitch();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            WeaponSelecter = 3;
            WeaponSwitch();
        }

        // Handle wrench activation input
        if (Input.GetKeyDown(KeyCode.F))
        {
            WrenchEnable();
        }

       
           
            

           
        

    }

    public void WrenchEnable()
    {

        Wrench_.gameObject.SetActive(true);
        DeactivateWeapons();
    }

    public void HackerItemEnable()
    {

        HackerItem.gameObject.SetActive(true);
        DeactivateWeapons();
    }

    public void HypershotEnable()
    {

        Hypershot_.gameObject.SetActive(true);
        DeactivateWeapons();
    }


    private void DeactivateWeapons()
    {
        foreach (Transform weapon in Weapons)
        {
            weapon.gameObject.SetActive(false);
        }
    }

    
    public void WeaponSwitch()
    {
        // Deactivate the wrench
        Wrench_.gameObject.SetActive(false);
        if(HackerItem != null)
        {
            HackerItem.gameObject.SetActive(false);
        }
        if (Hypershot_ != null)
        {
            Hypershot_.gameObject.SetActive(false);
        }

        // Activate the selected weapon and deactivate others
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (i == WeaponSelecter)
            {
                Weapons[i].gameObject.SetActive(true);
            }
            else
            {
                Weapons[i].gameObject.SetActive(false);
            }

            
        }

        GameObject.FindObjectOfType<UISight3D>().UseSight(WeaponSelecter);
    }


}
