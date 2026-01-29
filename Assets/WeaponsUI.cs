using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class WeaponsUI : MonoBehaviour
{

    Image image_;
    public Sprite WeaponImg;
    public WeaponUICanvas weaponUI;
    public int WeaponID;
    public static WeaponsUI WeaponsUI_;
    public int Level = 1;
    public float levelAmount;
    public int MaxAmmolvl;
    int textlvl;
    public string[] UpgradeInfoLevel;
    public string WeaponName;

    // Start is called before the first frame update
    void Start()
    {

        WeaponName = gameObject.name;

       

           // levelAmount = PlayerPrefs.GetFloat("LevelAmounts_" + WeaponID, levelAmount);
        //Level = PlayerPrefs.GetInt("Levels_" + WeaponID, Level);
        textlvl = PlayerPrefs.GetInt("Textlv" + WeaponID, textlvl);

        WeaponsUI_ = GetComponent<WeaponsUI>();



        GameObject.FindObjectOfType<WeaponUICanvas>().img.Add(GetComponent<WeaponsUI>());
    }


    private void OnEnable()
    {
        weaponUI = GameObject.FindObjectOfType<WeaponUICanvas>();



        


        weaponUI.images[WeaponID].enabled = true;

        weaponUI.images[WeaponID].sprite = WeaponImg;

    }

    private void OnDisable()
    {
        //weaponUI = GameObject.FindObjectOfType<WeaponUICanvas>();
        //GameObject.FindObjectOfType<WeaponUICanvas>().img.Remove(GetComponent<WeaponsUI>());
        //weaponUI.images[WeaponID].enabled = false;

    }


    // Update is called once per frame
    private void Update()
    {


        if (LevelWeapon.levelWeapon_.Level_Slider.fillAmount == 1)
        {



            if (Level < 5)
            {
                LevelWeapon.levelWeapon_.Level_Slider.fillAmount = 0;
                Level += 1;
                levelAmount = 0;
                StartCoroutine(Wait());
                GetComponent<WeaponAmmos>().MaxAmmo += MaxAmmolvl;
                GetComponent<WeaponAmmos>().Ammo += 80;
                //  PlayerPrefs.SetInt("Levels_" + WeaponID, Level);


                // PlayerPrefs.Save();


            }
            else
            {
                LevelWeapon.levelWeapon_.Level_Slider.fillAmount = 1;
            }

            if (Level == 5)
            {
                LevelWeapon.levelWeapon_.Level_Slider.fillAmount = 1;

            }
        }

        if (Level >= 5)
        {
            LevelWeapon.levelWeapon_.Level_Slider.fillAmount = 1;
        }


        WeaponAmmoCount.WeaponAmmoCount_.weaponIcone.sprite = WeaponImg;
        

        for (int i = 0; i < WeaponUICanvas.weaponcanvas_.img.Count(); i++)
        {
            WeaponUICanvas.weaponcanvas_.images[i].enabled = true;
        }

        //LevelWeapon.levelWeapon_.Level_Slider.fillAmount = levelAmount;
        // Save the updated levelAmount and Level back to PlayerPrefs.

       // PlayerPrefs.SetFloat("LevelAmounts_" + WeaponID, levelAmount);

        
       

    }


    public void Selected()
    {
       
        
        
      
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3);
        UpgradeInfo.UpgradeInfo_.UpgradeObj.SetActive(true);
        UpgradeInfo.UpgradeInfo_.UpgradeText.text = UpgradeInfoLevel[textlvl] + "  V" + Level + " !";
        textlvl += 1;
        PlayerPrefs.SetFloat("Textlv" + WeaponID, textlvl);
    }
}
