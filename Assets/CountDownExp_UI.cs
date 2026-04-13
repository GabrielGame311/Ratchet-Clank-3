using UnityEngine;
using TMPro; // Viktigt! Krävs för TextMeshPro

public class CountDownExp_UI : MonoBehaviour
{
    [Header("Timer Settings")]
    public float timeRemaining = 60f; // 1 minut
    public bool timerIsRunning = false;

    [Header("UI Elements")]
    public TextMeshProUGUI timeText; // Dra in din TMP-text här
    public Color warningColor = Color.red;
    public float warningThreshold = 10f; // När ska texten bli röd?

    private Color originalColor;

    void Start()
    {
        // Starta timern direkt
        timerIsRunning = true;

        if (timeText != null)
            originalColor = timeText.color;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);

                // Ratchet-effekt: Bli röd när det är nära slutet
                if (timeRemaining <= warningThreshold)
                {
                    timeText.color = warningColor;
                }
            }
            else
            {
                Debug.Log("BOOM! Zeldrin Starport exploderade!");
                timeRemaining = 0;
                timerIsRunning = false;
                Explode(); // Valfri metod för vad som händer vid 0
            }
        }

        if (timeRemaining < 5f)
        {
            // Enkelt blinkande baserat på sinusvåg
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * 10f));
            timeText.alpha = alpha;
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        // Vi räknar ut hela sekunder
        float seconds = Mathf.FloorToInt(timeToDisplay);

        // Vi räknar ut millisekunder (egentligen hundradelar, 00-99, för bäst effekt)
        // Vi tar decimalerna från timeToDisplay och gör om till heltal
        float fraction = timeToDisplay % 1;
        float milliseconds = Mathf.FloorToInt(fraction * 100);

        // Formaterar till SS:MS (t.ex. 60:00, 59:99, 59:98...)
        timeText.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);

        // Bonus: Gör texten röd när det är under 10 sekunder kvar
        if (timeRemaining < 10f)
        {
            timeText.color = warningColor;
        }
    }

    void Explode()
    {
        timeText.text = "00:00";
        Debug.Log("Skeppet exploderade! Uppdrag misslyckat.");
        // Här kan du trigga din döds-animation eller ladda om scenen
    }
}