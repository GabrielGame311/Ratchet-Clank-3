using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider slider;


    public TMP_Text HealthText_;


    public static HealthBar HealthBar_;


    public TMP_FontAsset Front_HP;
    public TMP_FontAsset Front_MaxHP;
    public int Front_HPSize = 22;      // Storlek för "200"
    public int Front_MaxHPSize = 14;   // Storlek för "/200"

    private void Start()
    {
        HealthBar_ = GetComponent<HealthBar>();


       
    }


    private void Update()
    {
        
    }

    public void SetMaxHealth (int health)
    {
        
        slider.maxValue = health;
       // slider.value = health;
        UpdateHealthText(health, health);
    }

    private void UpdateHealthText(int currentHealth, int maxHealth)
    {
        // Se till att siffrorna alltid har minst 2 eller 3 siffror (för snyggare UI)
        string hpText = currentHealth.ToString("D3"); // Gör om 1 till 001, 10 till 010 osv.
        string maxHpText = maxHealth.ToString("D3");

        // Sätter text med RÄTT storlek
        HealthText_.text = $"<size={Front_HPSize}>{hpText}</size> <size={Front_MaxHPSize}>/ {maxHpText}</size>";

        // Byt font genom att ändra Material Preset manuellt
        HealthText_.font = Front_HP; // Byt font för hela texten (se steg 2 nedan!)
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }


}
