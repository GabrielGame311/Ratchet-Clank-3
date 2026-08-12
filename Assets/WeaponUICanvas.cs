using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUICanvas : MonoBehaviour
{
    public static WeaponUICanvas Instance;
    public static WeaponUICanvas weaponcanvas_ => Instance; // Bakåtkompatibilitet

    [Header("UI Wheel Elements")]
    public List<Image> images = new List<Image>();
    public List<WeaponsUI> weaponsInWheel = new List<WeaponsUI>();

    [Header("Referens till Spelarens Vapen-objekt")]
    [Tooltip("Dra in 'Weapons' eller 'Hand' från Hierarchy här!")]
    public Transform weaponHolder;

    public int weaponSelect = 0;
    private int lastSelectedWeapon = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetupWeaponWheel();
    }

    public void SetupWeaponWheel()
    {
        weaponsInWheel.Clear();

        // 1. Dölj alla platser i hjulet från början
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i] != null) images[i].enabled = false;
        }

        // Försök hitta weaponHolder automatiskt om den inte är manuellt insatt
        if (weaponHolder == null && WeaponSwitcher.WeaponSwitcher_ != null)
        {
            weaponHolder = WeaponSwitcher.WeaponSwitcher_.transform;
        }

        if (weaponHolder == null)
        {
            Debug.LogError("[WeaponUICanvas] Du måste dra in ditt 'Weapons'-objekt till 'Weapon Holder' i Inspector!");
            return;
        }

        // 2. Hämta BARA vapen som ligger under spelaren (även inaktiva)
        WeaponsUI[] playerWeapons = weaponHolder.GetComponentsInChildren<WeaponsUI>(true);

        // 3. Lägg till vapnen i hjulet och koppla klick-funktioner
        foreach (WeaponsUI ui in playerWeapons)
        {
            weaponsInWheel.Add(ui);

            Sprite icon = ui.weaponImg;
            int targetID = ui.weaponID;

            if (icon != null && targetID >= 0 && targetID < images.Count)
            {
                images[targetID].sprite = icon;
                images[targetID].enabled = true;

                Button btn = images[targetID].GetComponent<Button>();
                if (btn == null)
                {
                    btn = images[targetID].gameObject.AddComponent<Button>();
                }

                // Koppla klicket direkt till SwitchWeapon
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SwitchWeapon(targetID));
            }
        }
    }

    /// <summary>
    /// Byter till vald vapen-slot och meddelar WeaponSwitcher
    /// </summary>
    public void SwitchWeapon(int wpIndex)
    {
        if (wpIndex < 0 || wpIndex >= images.Count) return;

        weaponSelect = wpIndex;

        if (weaponSelect != lastSelectedWeapon && WeaponSwitcher.WeaponSwitcher_ != null)
        {
            WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = weaponSelect;
            WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
            lastSelectedWeapon = weaponSelect;
        }
    }
}