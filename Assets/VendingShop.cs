using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VendingShop : MonoBehaviour
{
    public static VendingShop Instance;

    [Header("UI References - Selected Weapon Preview")]
    public TMP_Text boltText;
    public TMP_Text priceText;
    public Image weaponIcon;
    public TMP_Text weaponAmmoText;

    [Header("UI References - Container & Panels")]
    public Transform contentContainer;
    public GameObject shopVendorButtonPrefab;
    public GameObject weaponAmmoAllPanel;

    [Header("Audio & Settings")]
    public AudioSource soundSource;
    public AudioClip soundShop;
    public AudioClip soundError;

    [Header("Data")]
    public List<WeaponAmmos> weaponAmmosList = new List<WeaponAmmos>();

    private Bolts playerBolts;
    private WeaponAmmos currentlySelectedWeapon;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        playerBolts = FindObjectOfType<Bolts>();
        if (soundSource == null) soundSource = GetComponent<AudioSource>();

        RefreshWeaponList();
    }

    public void RefreshWeaponList()
    {
        weaponAmmosList.Clear();
        
        // Hittar alla unika vapen i scenen
        WeaponAmmos[] foundWeapons = FindObjectsOfType<WeaponAmmos>(true);
        HashSet<string> addedNames = new HashSet<string>();

        foreach (WeaponAmmos wp in foundWeapons)
        {
            if (wp == null) continue;

            if (!addedNames.Contains(wp.name))
            {
                addedNames.Add(wp.name);
                weaponAmmosList.Add(wp);
            }
        }

        BuildShopButtons();
        UpdateShopUI();
    }

    public void BuildShopButtons()
    {
        if (contentContainer == null || shopVendorButtonPrefab == null) return;

        // Rensa alla gamla knappar
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        bool anyWeaponNeedsAmmo = false;
        WeaponAmmos firstValidWeapon = null;

        foreach (WeaponAmmos wp in weaponAmmosList)
        {
            if (wp == null) continue;

            // FELSÖKNING: Skriver ut värdena i Console i Unity!
            Debug.Log($"[VendingShop] Kollar {wp.name}: Ammo = {wp.Ammo} / MaxAmmo = {wp.MaxAmmo}");

            // ⭐ HOPPA ÖVER VAPNET OM DET HAR FULL AMMO ELLER MaxAmmo = 0 ⭐
            if (wp.MaxAmmo <= 0 || wp.Ammo >= wp.MaxAmmo)
            {
                continue; 
            }

            anyWeaponNeedsAmmo = true;
            if (firstValidWeapon == null) firstValidWeapon = wp;

            // Skapa endast knapp om vapnet faktiskt behöver ammo
            GameObject newButton = Instantiate(shopVendorButtonPrefab, contentContainer);
            newButton.SetActive(true);

            var shopUI = newButton.GetComponent<Shop_UI>();
            var weaponUI = wp.GetComponent<WeaponsUI>();

            if (shopUI != null && shopUI.Icone != null && weaponUI != null)
            {
                shopUI.Icone.sprite = weaponUI.WeaponImg;
                shopUI.Icone.enabled = weaponUI.WeaponImg != null;
            }

            TMP_Text btnText = newButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
            {
                btnText.text = wp.name;
            }

            Button btnComp = newButton.GetComponent<Button>();
            if (btnComp != null)
            {
                btnComp.onClick.RemoveAllListeners();
                WeaponAmmos localWp = wp;
                btnComp.onClick.AddListener(() => SelectWeapon(localWp));
            }
        }

        if (firstValidWeapon != null)
        {
            SelectWeapon(firstValidWeapon);
        }
        else
        {
            ClearPreview();
        }

        if (weaponAmmoAllPanel != null)
        {
            weaponAmmoAllPanel.SetActive(anyWeaponNeedsAmmo);
        }
    }

    public void SelectWeapon(WeaponAmmos weapon)
    {
        if (weapon == null) return;

        currentlySelectedWeapon = weapon;
        var weaponUI = weapon.GetComponent<WeaponsUI>();

        if (weaponIcon != null && weaponUI != null)
        {
            weaponIcon.sprite = weaponUI.WeaponImg;
            weaponIcon.enabled = weaponUI.WeaponImg != null;
        }

        if (priceText != null)
        {
            priceText.text = $"Cost: {weapon.havePrice}";
        }

        if (weaponAmmoText != null)
        {
            weaponAmmoText.text = $"{weapon.Ammo:D2}/{weapon.MaxAmmo:D2}";
        }
    }

    private void ClearPreview()
    {
        currentlySelectedWeapon = null;
        if (weaponIcon != null) weaponIcon.enabled = false;
        if (priceText != null) priceText.text = "Cost: 0";
        if (weaponAmmoText != null) weaponAmmoText.text = "FULL";
    }

    public void UpdateShopUI()
    {
        if (playerBolts == null) playerBolts = FindObjectOfType<Bolts>();

        if (playerBolts != null && boltText != null)
        {
            boltText.text = playerBolts.bolt.ToString();
        }
    }

    public void BuySelectedWeapon()
    {
        if (currentlySelectedWeapon == null) return;
        BuySingleWeaponAmmo(currentlySelectedWeapon);
    }

    public void BuySingleWeaponAmmo(WeaponAmmos weapon)
    {
        if (weapon == null) return;
        if (playerBolts == null) playerBolts = FindObjectOfType<Bolts>();

        if (weapon.Ammo < weapon.MaxAmmo && playerBolts != null && playerBolts.bolt >= weapon.havePrice)
        {
            playerBolts.bolt -= weapon.havePrice;
            weapon.RefillAmmo();

            PlaySound(soundShop);

            BuildShopButtons();
            UpdateShopUI();
        }
        else
        {
            PlaySound(soundError);
        }
    }

    public void BuyAllAmmo()
    {
        if (playerBolts == null) playerBolts = FindObjectOfType<Bolts>();
        if (playerBolts == null) return;

        int totalCost = 0;
        List<WeaponAmmos> weaponsToRefill = new List<WeaponAmmos>();

        foreach (WeaponAmmos wp in weaponAmmosList)
        {
            if (wp != null && wp.Ammo < wp.MaxAmmo)
            {
                totalCost += wp.havePrice;
                weaponsToRefill.Add(wp);
            }
        }

        if (weaponsToRefill.Count == 0) return;

        if (playerBolts.bolt >= totalCost)
        {
            playerBolts.bolt -= totalCost;

            foreach (WeaponAmmos wp in weaponsToRefill)
            {
                wp.RefillAmmo();
            }

            PlaySound(soundShop);

            BuildShopButtons();
            UpdateShopUI();
        }
        else
        {
            PlaySound(soundError);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (soundSource != null && clip != null)
        {
            soundSource.PlayOneShot(clip);
        }
    }
}