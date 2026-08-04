using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsUI : MonoBehaviour
{
    public Sprite WeaponImg;
    public WeaponUICanvas weaponUI;
    public int WeaponID;
    public static WeaponsUI WeaponsUI_;

    [Header("Level Settings")]
    public int Level = 1;
    public float levelAmount;
    public int MaxAmmolvl;
    private int textlvl;
    
    public string[] UpgradeInfoLevel;
    public string WeaponName;

    private WeaponAmmos weaponAmmos;

    void Awake()
    {
        WeaponsUI_ = this;
        weaponAmmos = GetComponent<WeaponAmmos>();
    }

    void Start()
    {
        WeaponName = gameObject.name;
        textlvl = PlayerPrefs.GetInt("Textlv" + WeaponID, 0);
    }

    private void OnEnable()
    {
        if (WeaponUICanvas.weaponcanvas_ != null)
        {
            weaponUI = WeaponUICanvas.weaponcanvas_;
        }
        else
        {
            weaponUI = FindObjectOfType<WeaponUICanvas>();
        }

        // BARA när detta specifika vapen AKTIVERAS (tas fram) sätter vi dess ikon i ammunitionsfönstret:
        if (WeaponAmmoCount.WeaponAmmoCount_ != null && WeaponImg != null)
        {
            WeaponAmmoCount.WeaponAmmoCount_.weaponIcone.sprite = WeaponImg;
        }
    }

    private void Update()
    {
        // Level-up logik
        if (LevelWeapon.levelWeapon_ != null)
        {
            if (LevelWeapon.levelWeapon_.Level_Slider.fillAmount >= 1f)
            {
                if (Level < 5)
                {
                    LevelWeapon.levelWeapon_.Level_Slider.fillAmount = 0;
                    Level += 1;
                    levelAmount = 0;

                    if (weaponAmmos != null)
                    {
                        weaponAmmos.MaxAmmo += MaxAmmolvl;
                        weaponAmmos.Ammo += 80;
                    }

                    StartCoroutine(Wait());
                }
                else
                {
                    LevelWeapon.levelWeapon_.Level_Slider.fillAmount = 1f;
                }
            }

            if (Level >= 5)
            {
                LevelWeapon.levelWeapon_.Level_Slider.fillAmount = 1f;
            }
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3f);

        if (UpgradeInfo.UpgradeInfo_ != null)
        {
            UpgradeInfo.UpgradeInfo_.UpgradeObj.SetActive(true);
            
            if (textlvl < UpgradeInfoLevel.Length)
            {
                UpgradeInfo.UpgradeInfo_.UpgradeText.text = UpgradeInfoLevel[textlvl] + " V" + Level + " !";
            }

            textlvl += 1;
            PlayerPrefs.SetInt("Textlv" + WeaponID, textlvl);
            PlayerPrefs.Save();
        }
    }
}