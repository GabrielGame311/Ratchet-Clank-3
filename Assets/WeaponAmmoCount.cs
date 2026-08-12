using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponAmmoCount : MonoBehaviour
{
    public static WeaponAmmoCount Instance;
    public static WeaponAmmoCount WeaponAmmoCount_ => Instance; // Bakåtkompatibilitet för äldre skript

    [Header("UI References")]
    public TMP_Text ammoText;
    public Image weaponIcon;

    [Header("Current Weapon")]
    public WeaponAmmos currentWeapon;

    private WeaponsUI currentWeaponUI;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        UpdateHUD();
    }

    /// <summary>
    /// Uppdaterar ammunitionsräknaren i HUD:en varje bildruta
    /// </summary>
    public void UpdateHUD()
    {
        if (currentWeapon == null) return;

        if (ammoText != null)
        {
            ammoText.text = $"{currentWeapon.Ammo:D2} / {currentWeapon.MaxAmmo:D2}";
        }
    }

    /// <summary>
    /// Anropas när spelaren byter vapen
    /// </summary>
    public void SetActiveWeapon(WeaponAmmos newWeapon)
    {
        currentWeapon = newWeapon;

        if (currentWeapon != null)
        {
            // Cacha WeaponsUI en gång vid vapenbyte
            currentWeaponUI = currentWeapon.GetComponent<WeaponsUI>();

            if (weaponIcon != null && currentWeaponUI != null)
            {
                if (currentWeaponUI.weaponImg != null)
                {
                    weaponIcon.sprite = currentWeaponUI.weaponImg;
                }
            }
        }

        UpdateHUD();
    }
}