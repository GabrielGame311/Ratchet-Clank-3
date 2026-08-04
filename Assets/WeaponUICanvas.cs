using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUICanvas : MonoBehaviour
{
    public List<Image> images = new List<Image>();
    public List<WeaponsUI> img = new List<WeaponsUI>();

    [Header("Referens till Spelarens Vapen-objekt")]
    [Tooltip("Dra in 'Weapons' eller 'Hand' från Hierarchy här!")]
    public Transform weaponHolder;

    public int WeaponSelect = 0;
    public static WeaponUICanvas weaponcanvas_;
    private int lastSelectedWeapon = -1;

    void Awake()
    {
        weaponcanvas_ = this;
    }

    void Start()
    {
        SetupWeaponWheel();
    }

    public void SetupWeaponWheel()
{
    img.Clear();

    // 1. Dölj alla platser i hjulet från början
    for (int i = 0; i < images.Count; i++)
    {
        if (images[i] != null) images[i].enabled = false;
    }

    // Om weaponHolder inte är tilldelad, försök hitta spelarens WeaponSwitcher
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

    // 3. Lägg till vapnen i hjulet och koppla klick-funktion automatiskt
    foreach (WeaponsUI ui in playerWeapons)
    {
        img.Add(ui);

        if (ui.WeaponImg != null && ui.WeaponID >= 0 && ui.WeaponID < images.Count)
        {
            int targetID = ui.WeaponID; // Spara ID för knappen

            images[targetID].sprite = ui.WeaponImg;
            images[targetID].enabled = true;

            // Gör bilden klickbar automatiskt
            Button btn = images[targetID].GetComponent<Button>();
            if (btn == null)
            {
                btn = images[targetID].gameObject.AddComponent<Button>();
            }

            // Koppla klicket till SwitchWeapon
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SwitchWeapon(targetID));
        }
    }
}

    void Update()
    {
        if (WeaponSelect >= 0 && WeaponSelect <= 7)
        {
            if (WeaponSwitcher.WeaponSwitcher_ != null && WeaponSelect != lastSelectedWeapon)
            {
                WeaponSwitcher.WeaponSwitcher_.WeaponSelecter = WeaponSelect;
                WeaponSwitcher.WeaponSwitcher_.WeaponSwitch();
                lastSelectedWeapon = WeaponSelect;
            }
        }
    }

    public void SwitchWeapon(int wp)
    {
        WeaponSelect = wp;
    }
}