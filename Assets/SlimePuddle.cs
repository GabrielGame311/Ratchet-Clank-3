using UnityEngine;

public class SlimePuddle : MonoBehaviour
{
    public float lifetime = 4f;        // Hur länge pölen ligger kvar
    public float fadeSpeed = 2f;       // Hur snabbt den tonar ut i slutet
    public Vector3 targetScale = new Vector3(1f, 0.01f, 1f); // Hur stor pölen ska bli

    [Header("Höjdjustering (Y-led)")]
    public float yOffset = -0.02f;     // Sänk ner pölen lite för att undvika flimmer (Z-fighting) mot marken

    private Material puddleMaterial;
    private float timer = 0f;
    private Vector3 startScale;

    void Start()
    {
        // Starta väldigt liten (så det ser ut som den sprids ut på marken)
        startScale = new Vector3(0.1f, 0.01f, 0.1f);
        transform.localScale = startScale;

        // Sänk ner pölens position på Y-axeln direkt vid start
        Vector3 newPos = transform.position;
        newPos.y += yOffset;
        transform.position = newPos;

        // Hämta materialet (gör en kopia så vi inte tonar ut alla pölar samtidigt)
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            puddleMaterial = renderer.material;
        }

        // TVINGA PÖLEN ATT LIGGA PLATT:
        // Vi sätter X till 90 grader (så den ligger ner) och slumpar Y (rotationen på marken)
        float randomY = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0f, randomY, 0f);
    }


   

    void Update()
    {
        timer += Time.deltaTime;

        // 1. Skala upp pölen mjukt i början (växer ut som flytande vatten)
        if (timer < 0.5f)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, timer / 0.5f);
        }

        // 2. Tona ut materialets genomskinlighet när livstiden börjar ta slut
        if (timer > (lifetime - 1f) && puddleMaterial != null)
        {
            Color color = puddleMaterial.color;
            color.a = Mathf.MoveTowards(color.a, 0f, fadeSpeed * Time.deltaTime);
            puddleMaterial.color = color;

            // Skala även ner den lite under tiden den försvinner
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, fadeSpeed * Time.deltaTime);
        }

        // 3. Förstör objektet
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}