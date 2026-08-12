using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelWeapon : MonoBehaviour
{
    public static LevelWeapon Instance;
    public static LevelWeapon levelWeapon_ => Instance; // Bakåtkompatibilitet

    [Header("UI Elements")]
    public Image Level_Slider;
    public TMP_Text levelcount;
    public GameObject upgrade;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Level_Slider != null)
        {
            Level_Slider.fillAmount = 0f;
        }

        if (upgrade != null)
        {
            upgrade.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateLevelUI();
    }

    /// <summary>
    /// Uppdaterar UI-stapeln och texten för det aktuella aktivt valda vapnet
    /// </summary>
    private void UpdateLevelUI()
    {
        // Hämta det aktiva vapnet från HUD-hanteraren
        if (WeaponAmmoCount.Instance == null || WeaponAmmoCount.Instance.currentWeapon == null)
            return;

        WeaponsUI activeWeaponUI = WeaponAmmoCount.Instance.currentWeapon.GetComponent<WeaponsUI>();
        if (activeWeaponUI == null) return;

        // Uppdatera level-text (t.ex. "V1", "V2")
        if (levelcount != null)
        {
            levelcount.text = $"V{activeWeaponUI.level}";
        }

        // Uppdatera XP-slidern
        if (Level_Slider != null && activeWeaponUI.xpRequiredForNextLevel > 0)
        {
            Level_Slider.fillAmount = activeWeaponUI.currentXP / activeWeaponUI.xpRequiredForNextLevel;
        }
    }

    /// <summary>
    /// Visar uppgraderings-popup och pausar spelet tillfälligt
    /// </summary>
    public void TriggerUpgradeEffect()
    {
        StartCoroutine(UpgradeRoutine());
    }

    private IEnumerator UpgradeRoutine()
    {
        if (upgrade != null) upgrade.SetActive(true);

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;

        if (upgrade != null) upgrade.SetActive(false);
    }
}