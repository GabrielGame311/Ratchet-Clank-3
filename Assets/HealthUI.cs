using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour
{

    // En array med bilder för att representera hälsa i steg om 20
    public GameObject[] HealthImages;
    public int health;

    void Start()
    {
        // Initiera hälsan till 100
      
      
    }

    private void Update()
    {
        health = GameObject.FindObjectOfType<QuarkController>().Health;
    }


    public void UpdateHealthUI()
    {
        // Gå igenom varje objekt i HealthImages
        
        for (int i = 0; i < HealthImages.Length; i++)
        {
            // Bestäm vid vilket hälsovärde varje bild ska aktiveras/deaktiveras
            int healthThreshold = (HealthImages.Length - i) * 20;

            // Sätt bilden till aktiv om hälsan är större än eller lika med gränsvärdet, annars inaktiv
            HealthImages[i].SetActive(health >= healthThreshold);
        }
    }
}
