using System.Collections.Generic;
using UnityEngine;

public class MapProgressionManager : MonoBehaviour
{
    [Header("Map Settings")]
    [Tooltip("Dra in parent-objektet där denna kartas fiender ligger")]
    public Transform enemyParent;

    [Tooltip("Dra in GameObjektet för nästa map som ska aktiveras")]
    public GameObject nextMap;

    [Header("Kill Requirements")]
    [Tooltip("Hur många fiender som måste dödas för att låsa upp nästa map (t.ex. 3)")]
    public int requiredKills = 3;

    private List<GameObject> trackedEnemies = new List<GameObject>();
    private int deadEnemyCount = 0;
    private bool mapUnlocked = false;
    
    public bool ISound = false;


    void Start()
    {
        // Se till att nästa map är avstängd från start (om du vill det)
        if (nextMap != null && nextMap.activeSelf)
        {
            nextMap.SetActive(false);
        }

        // Hämta alla fiender under enemyParent (även inaktiva)
        RegisterEnemies();
    }

    void RegisterEnemies()
    {
        if (enemyParent == null) return;

        trackedEnemies.Clear();
        Transform[] children = enemyParent.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in children)
        {
            // Kontrollera taggen (se till att stavningen matchar din tagg i Unity)
            if (t.CompareTag("Enemie") && t != enemyParent)
            {
                trackedEnemies.Add(t.gameObject);
            }
        }
    }

    void Update()
    {
        if (mapUnlocked) return;

        // Räkna hur många fiender i listan som har blivit förstörda (Destroyed) eller inaktiverade
        int currentDeadCount = 0;

        foreach (GameObject enemy in trackedEnemies)
        {
            // Om objektet är borttaget (null) eller inaktiverat (t.ex. vid död)
            if (enemy == null || !enemy.activeInHierarchy)
            {
                currentDeadCount++;
            }
        }

        // Om vi nått eller passerat antalet requiredKills
        if (currentDeadCount >= requiredKills)
        {
            UnlockNextMap();
            if(ISound)
            {
                MissionSound.MissionSound_.i++;
                MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
            }
        }
    }

    void UnlockNextMap()
    {
        mapUnlocked = true;

        if (nextMap != null)
        {
            nextMap.SetActive(true);
            Debug.Log($"Du har dödat tillräckligt med fiender! Aktiverar {nextMap.name}");
        }

        
    }

    // Alternativ: Anropa denna metod direkt från fiendens död-skript om du föredrar det
    public void OnEnemyKilled()
    {
        if (mapUnlocked) return;

        deadEnemyCount++;
        if (deadEnemyCount >= requiredKills)
        {
            UnlockNextMap();
        }
        
    }
}