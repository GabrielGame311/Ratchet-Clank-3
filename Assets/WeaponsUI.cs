using System.Collections;
using UnityEngine;

public class WeaponsUI : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName;
    public int weaponID;
    public Sprite weaponImg;

    [Header("Level Settings")]
    public int level = 1;
    public int maxLevel = 5;
    public float currentXP;
    public float xpRequiredForNextLevel = 100f;
    public Sprite WeaponImg => weaponImg;
    [Header("Upgrade Bonuses")]
    public int maxAmmoPerLevel = 20;
    public int ammoBonusOnLevelUp = 80;
    public string[] upgradeInfoLevel;

    private int textlvl;
    private WeaponAmmos weaponAmmos;

    private void Awake()
    {
        weaponAmmos = GetComponent<WeaponAmmos>();
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(weaponName))
        {
            weaponName = gameObject.name;
        }

        // Ladda sparad progresstext och level
        level = PlayerPrefs.GetInt($"WeaponLevel_{weaponID}", 1);
        textlvl = PlayerPrefs.GetInt($"Textlv_{weaponID}", 0);
    }

    private void OnEnable()
    {
        // Registrera detta vapen som det aktiva i HUD:en när det tas fram
        if (WeaponAmmoCount.Instance != null && weaponAmmos != null)
        {
            WeaponAmmoCount.Instance.SetActiveWeapon(weaponAmmos);
        }
    }

    /// <summary>
    /// Anropas när vapnet delar ut skada eller dödar fiender för att ge XP.
    /// </summary>
    public void AddXP(float amount)
    {
        if (level >= maxLevel) return;

        currentXP += amount;

        // Uppdatera Level-slidern i UI om den finns
        if (LevelWeapon.levelWeapon_ != null && LevelWeapon.levelWeapon_.Level_Slider != null)
        {
            LevelWeapon.levelWeapon_.Level_Slider.fillAmount = currentXP / xpRequiredForNextLevel;
        }

        // Kontrollera om vapnet går upp i level
        if (currentXP >= xpRequiredForNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        if (level >= maxLevel) return;

        level++;
        currentXP = 0f;

        // Återställ XP-slidern
        if (LevelWeapon.levelWeapon_ != null && LevelWeapon.levelWeapon_.Level_Slider != null)
        {
            LevelWeapon.levelWeapon_.Level_Slider.fillAmount = (level >= maxLevel) ? 1f : 0f;
        }

        // Höj max ammo och ge bonusammunition
        if (weaponAmmos != null)
        {
            weaponAmmos.MaxAmmo += maxAmmoPerLevel;
            weaponAmmos.Ammo = Mathf.Min(weaponAmmos.Ammo + ammoBonusOnLevelUp, weaponAmmos.MaxAmmo);
        }

        // Spara ny level
        PlayerPrefs.SetInt($"WeaponLevel_{weaponID}", level);
        PlayerPrefs.Save();

        // Visa uppgraderingsnotifikation
        StartCoroutine(ShowUpgradeInfoRoutine());
    }

    private IEnumerator ShowUpgradeInfoRoutine()
    {
        yield return new WaitForSeconds(3f);

        if (UpgradeInfo.UpgradeInfo_ != null)
        {
            UpgradeInfo.UpgradeInfo_.UpgradeObj.SetActive(true);

            if (upgradeInfoLevel != null && textlvl < upgradeInfoLevel.Length)
            {
                UpgradeInfo.UpgradeInfo_.UpgradeText.text = $"{upgradeInfoLevel[textlvl]} V{level} !";
            }

            textlvl++;
            PlayerPrefs.SetInt($"Textlv_{weaponID}", textlvl);
            PlayerPrefs.Save();
        }
    }
}