using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAmmos : MonoBehaviour
{
    public int Ammo;
    public int MaxAmmo;
    public bool Gun = true;
    public int WeaponID;
    Animator anime;
    public int havePrice;
    public bool buttonInstantiated = false;
    public string names;
    public GameObject buttonInstance;
  
    // Start is called before the first frame update
    void Start()
    {

       // MaxAmmo = PlayerPrefs.GetInt("MaxAmmo" + GetComponent<WeaponsUI>().WeaponID, MaxAmmo);
      //  Ammo = PlayerPrefs.GetInt("Ammo" + GetComponent<WeaponsUI>().WeaponID, Ammo);
        MaxAmmo = Ammo;
        anime = GetComponentInParent<Animator>();
    }

    public void RefillAmmo()
    {
        Ammo += MaxAmmo;
        if (Ammo > MaxAmmo)
        {
            Ammo = MaxAmmo;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Ammo > MaxAmmo)
        {
            Ammo = MaxAmmo; // Limit Ammo to MaxAmmo
            PlayerPrefs.SetInt("MaxAmmo" + GetComponent<WeaponsUI>().WeaponID, MaxAmmo);
            PlayerPrefs.SetInt("Ammo" + GetComponent<WeaponsUI>().WeaponID, Ammo);
        }
        

        if (Gun)
        {
            anime.SetBool("Gun", true);
        }
        else
        {
            anime.SetBool("Gun", false);
        }
    }

    
}
