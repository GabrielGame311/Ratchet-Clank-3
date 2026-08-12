using UnityEngine;

public class WeaponAmmos : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int maxAmmo;
    public int ammo; // Sätts till maxAmmo som standard
    public int havePrice;

    [Header("Weapon Type")]
    public bool isGun = true;

    // Egenskaper för bakåtkompatibilitet
    public int Ammo { get => ammo; set => ammo = value; }
    public int MaxAmmo { get => maxAmmo; set => maxAmmo = value; }

    private Animator anime;
    private WeaponsUI weaponsUI;

    private void Awake()
    {
        anime = GetComponentInParent<Animator>();
        weaponsUI = GetComponent<WeaponsUI>();

        // Ladda sparad ammo från PlayerPrefs eller sätt till MaxAmmo
        LoadAmmoData();

        if (anime != null)
        {
            anime.SetBool("Gun", isGun);
        }
    }

    /// <summary>
    /// Laddar sparad ammo från PlayerPrefs vid start
    /// </summary>
    public void LoadAmmoData()
    {
        if (weaponsUI != null)
        {
            if (PlayerPrefs.HasKey($"Ammo_{weaponsUI.weaponID}"))
            {
                ammo = PlayerPrefs.GetInt($"Ammo_{weaponsUI.weaponID}");
                maxAmmo = PlayerPrefs.GetInt($"MaxAmmo_{weaponsUI.weaponID}", maxAmmo);
            }
            else
            {
                // Om inget finns sparat startar vapnet med fullt magasin
                ammo = maxAmmo;
            }
        }
        else
        {
            // Säkerhetskoll om WeaponsUI saknas
            if (ammo <= 0 && maxAmmo > 0)
            {
                ammo = maxAmmo;
            }
        }
    }

    /// <summary>
    /// Fyller på alla skott till MaxAmmo
    /// </summary>
    public void RefillAmmo()
    {
        ammo = maxAmmo;
        SaveAmmoData();
    }

    /// <summary>
    /// Avfyrar ett skott om det finns ammunition kvar
    /// </summary>
    public bool TryShoot()
    {
        if (ammo <= 0)
        {
            Debug.Log("Slut på ammo!");
            return false;
        }

        ammo--;

        if (anime != null)
        {
            anime.SetTrigger("Shoot");
        }

        SaveAmmoData();
        return true;
    }

    /// <summary>
    /// Sparar ammo säkert i PlayerPrefs
    /// </summary>
    public void SaveAmmoData()
    {
        if (weaponsUI != null)
        {
            PlayerPrefs.SetInt($"MaxAmmo_{weaponsUI.weaponID}", maxAmmo);
            PlayerPrefs.SetInt($"Ammo_{weaponsUI.weaponID}", ammo);
            PlayerPrefs.Save();
        }
    }
}