using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Torrets : MonoBehaviour
{
    public Camera playerCamera;
    public float baseDistance = 15f; // Avstånd framför kameran för att kontrollera marken
    public float groundOffset = 0.1f; // Offset från marken för att undvika "nudd"
    public float rotationSpeed = 60f; // Rotationshastighet för holografisk effekt
    public float pulseSpeed = 2f; // Hastighet för pulsation
    public float minScale = 0.1f; // Minsta storlek för pulsation
    public float maxScale = 0.2f; // Största storlek för pulsation

    private Renderer sightRenderer;
    private Color sightColor = Color.cyan; // Holografisk cyanfärg

    void Start()
    {
        sightRenderer = GetComponent<Renderer>();
        if (sightRenderer == null)
        {
            Debug.LogError("Siktet behöver en Renderer-komponent!");
        }
        sightRenderer.material.color = sightColor;

        // Återställ siktets position till origo
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one * minScale; // Starta med minsta storlek
    }

    void Update()
    {
        if (playerCamera == null) return;

        // Placera siktet på marken under punkt där baseDistance träffar marken
        Vector3 groundCheckPosition = playerCamera.transform.position + playerCamera.transform.forward * baseDistance;
        if (Physics.Raycast(groundCheckPosition, Vector3.down, out RaycastHit groundHit, 100f))
        {
            // Placera siktet på marken med en liten offset uppåt
            transform.position = groundHit.point + Vector3.up * groundOffset;
        }
        else
        {
            // Om ingen mark hittas, placera siktet på basavstånd framför kameran (som fallback)
            transform.position = playerCamera.transform.position + playerCamera.transform.forward * baseDistance;
        }

        // Vänd siktet mot kameran för att alltid vara synligt
        Vector3 cameraDirection = playerCamera.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(cameraDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Rotera siktet för sci-fi-känsla
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Pulsera storleken för holografisk effekt
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2);
        transform.localScale = Vector3.one * scale;
    }






}
