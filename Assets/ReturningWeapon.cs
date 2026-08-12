using UnityEngine;

public class ReturningWeapon : MonoBehaviour
{
    [Header("U-Curve Settings")]
    [Tooltip("Hur brett ut åt sidan U-kurvan ska böja (högre värde = bredare kurva).")]
    public float arcWidth = 6f;

    [Tooltip("Tid i sekunder för vapnet att flyga ut.")]
    public float timeOut = 0.7f;

    [Tooltip("Tid i sekunder för vapnet att flyga tillbaka.")]
    public float timeBack = 0.7f;

    [Header("Rotation Settings")]
    public Vector3 rotationAxis = new Vector3(0, 1, 0);
    public float spinSpeed = 1000f;

    private Transform ownerHand;
    private QuarkBoss bossScript;

    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 controlPointOut;

    private float timer = 0f;
    private bool isReturning = false;
    private Vector3 apexPosition;

    public void Initialize(Vector3 target, Transform hand, QuarkBoss boss)
    {
        ownerHand = hand;
        bossScript = boss;
        startPos = transform.position;
        targetPos = target;

        // Beräkna höger-riktningen i förhållande till kastet
        Vector3 throwDirection = (targetPos - startPos).normalized;
        Vector3 sideDirection = Vector3.Cross(throwDirection, Vector3.up).normalized;

        // Skapa en kontrollpunkt vid sidan för att skapa U-kurvan
        Vector3 midPoint = Vector3.Lerp(startPos, targetPos, 0.5f);
        controlPointOut = midPoint + (sideDirection * arcWidth);
    }

    void Update()
    {
        // 1. Snurra vapnet hela tiden
        transform.Rotate(rotationAxis * spinSpeed * Time.deltaTime, Space.Self);

        timer += Time.deltaTime;

        if (!isReturning)
        {
            // FAS 1: Flyg ut i U-bågen (Bezier-kurva)
            float t = Mathf.Clamp01(timer / timeOut);
            transform.position = GetBezierPoint(startPos, controlPointOut, targetPos, t);

            if (t >= 1f)
            {
                isReturning = true;
                timer = 0f;
                apexPosition = transform.position; // Spara positionen där hemresan börjar
            }
        }
        else
        {
            // FAS 2: Flyg tillbaka mot handen i en uppföljande kurva
            float t = Mathf.Clamp01(timer / timeBack);
            Vector3 currentHandPos = (ownerHand != null) ? ownerHand.position : startPos;

            // Beräkna returkurvans kontrollpunkt
            Vector3 returnDir = (currentHandPos - apexPosition).normalized;
            Vector3 sideDir = Vector3.Cross(returnDir, Vector3.up).normalized;
            Vector3 midPointBack = Vector3.Lerp(apexPosition, currentHandPos, 0.5f);
            Vector3 controlPointBack = midPointBack + (sideDir * (arcWidth * 0.5f));

            transform.position = GetBezierPoint(apexPosition, controlPointBack, currentHandPos, t);

            if (t >= 1f)
            {
                if (bossScript != null)
                {
                    bossScript.OnWeaponReturned();
                }
                Destroy(gameObject);
            }
        }
    }

    // Matematisk formel för Quadratic Bezier Curve
    private Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }
}