using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public static WeaponSwitcher Instance;
    public static WeaponSwitcher WeaponSwitcher_ => Instance; // Bakåtkompatibilitet

    [Header("Weapon List")]
    public List<Transform> Weapons = new List<Transform>();
    public List<WeaponsUI> WeaponsUI_ = new List<WeaponsUI>();
    public int WeaponSelecter = 0;

    [Header("Special Tools")]
    public Transform Wrench_;
    public Transform HackerItem;
    public Transform Hypershot_;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeWeapons();
        WeaponSwitch();
    }

    private void InitializeWeapons()
    {
        Weapons.Clear();
        WeaponsUI_.Clear();

        // Hämta alla WeaponsUI i spelarens hand/barnobjekt (även inaktiva)
        WeaponsUI[] foundUI = GetComponentsInChildren<WeaponsUI>(true);

        foreach (WeaponsUI wUI in foundUI)
        {
            if (wUI != null)
            {
                WeaponsUI_.Add(wUI);
                Weapons.Add(wUI.transform);
            }
        }
    }

    private void Update()
    {
        // Vapenval via siffertangenter (1-4)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectWeaponIndex(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectWeaponIndex(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectWeaponIndex(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectWeaponIndex(3);

        // Aktivera skiftnyckel (F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            WrenchEnable();
        }
    }

    public void SelectWeaponIndex(int index)
    {
        if (index >= 0 && index < Weapons.Count)
        {
            WeaponSelecter = index;
            WeaponSwitch();
        }
    }

    public void WeaponSwitch()
    {
        // Dölj specialverktyg
        if (Wrench_ != null) Wrench_.gameObject.SetActive(false);
        if (HackerItem != null) HackerItem.gameObject.SetActive(false);
        if (Hypershot_ != null) Hypershot_.gameObject.SetActive(false);

        // Aktivera endast det valda vapnet
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (Weapons[i] != null)
            {
                bool isSelected = (i == WeaponSelecter);
                Weapons[i].gameObject.SetActive(isSelected);
            }
        }

        // Uppdatera sikte om UISight3D finns i scenen
        UISight3D sight = FindObjectOfType<UISight3D>();
        if (sight != null)
        {
            sight.UseSight(WeaponSelecter);
        }
    }

    // --- Specialverktyg ---

    public void WrenchEnable()
    {
        DeactivateWeapons();
        if (Wrench_ != null) Wrench_.gameObject.SetActive(true);
    }

    public void HackerItemEnable()
    {
        DeactivateWeapons();
        if (HackerItem != null) HackerItem.gameObject.SetActive(true);
    }

    public void HypershotEnable()
    {
        DeactivateWeapons();
        if (Hypershot_ != null) Hypershot_.gameObject.SetActive(true);
    }

    public void DeactivateWeapons()
    {
        foreach (Transform weapon in Weapons)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(false);
            }
        }
    }

    public void ActivateWeapons()
    {
        WeaponSwitch();
    }
}