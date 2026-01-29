using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountTime : MonoBehaviour
{
    public TMP_Text timerText;  // Drag TMP-texten hit i Unity
    private float timer = 0f;          // Starttid för timern

    void Update()
    {
        // Öka timern med tiden som passerat sedan förra framen
        timer += Time.deltaTime;

        // Beräkna minuter och sekunder
        int minutes = Mathf.FloorToInt(timer / 60); // Konverterar tid till minuter
        int seconds = Mathf.FloorToInt(timer % 60); // Resterande sekunder

        // Uppdatera TextMeshPro-texten i formatet "0:01", "0:02", etc.
        timerText.text = $"{minutes}:{seconds:00}";
    }
}
