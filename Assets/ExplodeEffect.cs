using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;




public class ExplodeEffect : MonoBehaviour
{


    [Header("Materialinställningar")]
    public List<MeshRenderer> targetMaterials = new List<MeshRenderer>(); // Dra in alla material här
    [ColorUsage(true, true)] public Color hdrRedColor; // HDR-färg för glöd

    [Header("Animation")]
    public float pulseSpeed = 2f;
    public float minIntensity = 0f;
    public float maxIntensity = 2f;

    public int MusicPlaying;


    private void Start()
    {
        GameObject.FindObjectOfType<MusicPlay>().AudioPlay = MusicPlaying;
    }

    void Update()
    {
        // Skapa en mjuk sinusvåg mellan 0 och 1
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

        // Mappa om värdet till din önskade intensitet
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);

        // Loopa igenom alla material i listan och uppdatera dem
        foreach (MeshRenderer mat in targetMaterials)
        {
            if (mat != null)
            {
                // Vi ändrar EmissionColor för att få det att "lysa"
                mat.material.SetColor("_EmissionColor", hdrRedColor * currentIntensity);

                // Aktivera Emission-keywordet utifall det är avstängt
                mat.material.EnableKeyword("_EMISSION");
            }
        }
    }
}
