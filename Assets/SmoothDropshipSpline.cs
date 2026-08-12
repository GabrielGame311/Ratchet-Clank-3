using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SmoothDropshipSpline : MonoBehaviour
{
    [Header("Spline Settings")]
    [Tooltip("Spline-banan som skeppet följer (skickas oftast från Spawner-skriptet)")]
    public SplineContainer splinePath;

    [Tooltip("När på splinen skeppet ska stanna och släppa av fiender (0.5 = halvvägs på kurvan)")]
    [Range(0f, 1f)]
    public float dropPointTime = 0.5f;

    [Header("Movement Settings")]
    public float entrySpeed = 35f;
    public float turnSpeed = 8f;
    [Tooltip("Hur mycket skeppet lutar i kurvorna")]
    public float bankingAmount = 45f;

    [Header("R&C 3 Exit Boost Settings")]
    [Tooltip("Hur snabbt skeppet accelererar när det flyr längs slutet av splinen")]
    public float accelerationRate = 55f;
    [Tooltip("Toppfarten när skeppet dundrar iväg i rymden")]
    public float maxExitSpeed = 95f;

    [Header("Braking & Weight (Overshoot)")]
    [Tooltip("Hur mycket skeppet glider förbi landningspunkten innan det fjädrar tillbaka")]
    public float brakeOvershootAmount = 3.5f;
    [Tooltip("Hur snabbt skeppet fjädrar tillbaka till landningspunkten")]
    public float brakeSettleSpeed = 3f;

    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public Transform spawnPoint; // Var under skeppet fienderna ramlar ut
    public float timeBetweenEnemies = 0.4f;
    public float enemyLaunchForce = 14f;

    [Header("Scaling & Spawn Fix Settings")]
    public float scaleUpDuration = 0.5f;
    public float startScaleFactor = 0.15f;

    [Header("VFX & Juice")]
    public ParticleSystem[] thrusterParticles;

    private enum ShipState { Incoming, Braking, Dropping, Exiting, Finished }
    private ShipState currentState = ShipState.Incoming;

    private Animator anim;
    private float currentSpeed;
    private float currentDistanceAlongSpline = 0f;
    private float totalSplineLength = 1f;

    // Broms-variabler
    private Vector3 brakeStartPos;
    private Vector3 overshootTarget;
    private float brakeTimer = 0f;

    private Collider[] shipColliders;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        currentSpeed = entrySpeed;
        shipColliders = GetComponentsInChildren<Collider>();

        if (splinePath != null)
        {
            InitSpline(splinePath);
        }

        SetThrustersEmission(1f);
    }

    // Denna anropas automatiskt när spawner-objektet skapar skeppet
    public void InitSpline(SplineContainer path)
    {
        splinePath = path;
        totalSplineLength = splinePath.CalculateLength();
        currentDistanceAlongSpline = 0f;

        // Stäng av fysik/gravitation om skeppet har Rigidbody
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Tvinga positionen till startpunkten på splinen direkt
        Vector3 startPos = GetSplineWorldPosition(0f);
        Vector3 startTangent = GetSplineWorldTangent(0f);

        transform.position = startPos;
        if (startTangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(startTangent);
        }
    }
    void Update()
    {
        if (splinePath == null) return;

        switch (currentState)
        {
            case ShipState.Incoming:
                MoveAlongSpline();
                break;

            case ShipState.Braking:
                HandleBraking();
                break;

            case ShipState.Dropping:
                ApplyHoverEffect();
                break;

            case ShipState.Exiting:
                MoveAlongSplineExit();
                break;
        }
    }

    void MoveAlongSpline()
    {
        // Öka avståndet längs splinen
        currentDistanceAlongSpline += currentSpeed * Time.deltaTime;
        float currentT = Mathf.Clamp01(currentDistanceAlongSpline / totalSplineLength);

        // Hämta position och framåt-vektor från Splinen i World Space
        Vector3 targetPos = GetSplineWorldPosition(currentT);
        Vector3 forwardDir = GetSplineWorldTangent(currentT);

        transform.position = targetPos;
        ApplySplineRotation(forwardDir);

        // När vi når drop-punkten (t.ex. vid 50% av banan) -> Bromsa in!
        if (currentT >= dropPointTime)
        {
            SetupBrakeOvershoot();
        }
    }

    void MoveAlongSplineExit()
    {
        // Rocket Boost! Accelerera snabbt längs sista delen av splinen
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxExitSpeed, accelerationRate * Time.deltaTime);
        SetThrustersEmission(2.5f);

        currentDistanceAlongSpline += currentSpeed * Time.deltaTime;
        float currentT = Mathf.Clamp01(currentDistanceAlongSpline / totalSplineLength);

        Vector3 targetPos = GetSplineWorldPosition(currentT);
        Vector3 forwardDir = GetSplineWorldTangent(currentT);

        transform.position = targetPos;
        ApplySplineRotation(forwardDir);

        // När skeppet nått slutet på splinen -> Försvinn
        if (currentT >= 0.99f)
        {
            currentState = ShipState.Finished;
            Destroy(gameObject);
        }
    }

    void ApplySplineRotation(Vector3 forwardDir)
    {
        if (forwardDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(forwardDir);

            // Beräkna svängningsvinkel för banking (lutning i kurvor)
            float angleDiff = Vector3.SignedAngle(transform.forward, forwardDir, Vector3.up);
            float targetBank = Mathf.Clamp(angleDiff * 2.5f, -bankingAmount, bankingAmount);
            targetRot *= Quaternion.Euler(0, 0, -targetBank);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }
    }

    void SetupBrakeOvershoot()
    {
        currentState = ShipState.Braking;
        brakeTimer = 0f;
        brakeStartPos = transform.position;

        // Beräkna vart skeppet glider förbi utifrån tangenten vid landningspunkten
        Vector3 dropPos = GetSplineWorldPosition(dropPointTime);
        Vector3 tangent = GetSplineWorldTangent(dropPointTime);

        overshootTarget = dropPos + (tangent * brakeOvershootAmount);
    }

    void HandleBraking()
    {
        brakeTimer += Time.deltaTime * brakeSettleSpeed;
        Vector3 dropPos = GetSplineWorldPosition(dropPointTime);

        if (brakeTimer < 1f)
        {
            // Glid framåt mot overshootTarget med en mjuk sinus-kurva
            float t = Mathf.Sin(brakeTimer * Mathf.PI * 0.5f);
            transform.position = Vector3.Lerp(brakeStartPos, overshootTarget, t);
        }
        else
        {
            // Fjädra tillbaka till den exakta landningspunkten på splinen
            float t = brakeTimer - 1f;
            transform.position = Vector3.Lerp(overshootTarget, dropPos, t);

            if (brakeTimer >= 2f)
            {
                transform.position = dropPos;
                StartCoroutine(DropSequence());
            }
        }

        // Rikta in skeppet efter splinens framåt-vektor
        Vector3 tangent = GetSplineWorldTangent(dropPointTime);
        if (tangent != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(tangent);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }
    }

    void ApplyHoverEffect()
    {
        Vector3 dropPos = GetSplineWorldPosition(dropPointTime);

        // Nervöst och intensivt R&C3-hovrande
        float hover = Mathf.Sin(Time.time * 4.5f) * 0.35f;
        transform.position = dropPos + new Vector3(0, hover, 0);

        float roll = Mathf.Sin(Time.time * 3.5f) * 5f;
        float pitch = Mathf.Cos(Time.time * 3.0f) * 2.5f;

        Vector3 tangent = GetSplineWorldTangent(dropPointTime);
        Quaternion baseRot = tangent != Vector3.zero ? Quaternion.LookRotation(tangent) : transform.rotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            baseRot * Quaternion.Euler(pitch, 0, roll),
            Time.deltaTime * 6f
        );
    }

    IEnumerator DropSequence()
    {
        currentState = ShipState.Dropping;
        SetThrustersEmission(0.3f); // Minska motor-eld under hovring

        if (anim) anim.SetBool("Open", true);
        yield return new WaitForSeconds(0.8f);

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab != null)
            {
                GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                StartCoroutine(ScaleAndLaunchEnemy(enemy));
                yield return new WaitForSeconds(timeBetweenEnemies);
            }
        }

        yield return new WaitForSeconds(0.4f);
        if (anim) anim.SetBool("Open", false);
        yield return new WaitForSeconds(0.8f);

        StartExitSequence();
    }

    void StartExitSequence()
    {
        currentState = ShipState.Exiting;
        currentSpeed += 15f; // Ge en kick i starten när skeppet drar iväg!
    }

    IEnumerator ScaleAndLaunchEnemy(GameObject enemy)
    {
        if (enemy == null) yield break;

        Vector3 originalScale = enemy.transform.localScale;
        enemy.transform.localScale = originalScale * startScaleFactor;

        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        Collider enemyCollider = enemy.GetComponent<Collider>();

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
            if (shipColliders != null)
            {
                foreach (Collider shipCol in shipColliders)
                {
                    if (shipCol != null) Physics.IgnoreCollision(enemyCollider, shipCol, true);
                }
            }
        }

        if (enemyRb != null)
        {
#if UNITY_2023_1_OR_NEWER
            enemyRb.linearVelocity = Vector3.zero;
#else
            enemyRb.velocity = Vector3.zero;
#endif
            Vector3 pushDir = (transform.forward * 1.9f + Vector3.down * 0.1f).normalized;
            enemyRb.AddForce(pushDir * enemyLaunchForce, ForceMode.Impulse);
        }

        float timer = 0f;
        while (timer < scaleUpDuration)
        {
            if (enemy == null) yield break;

            timer += Time.deltaTime;
            float progress = timer / scaleUpDuration;
            enemy.transform.localScale = Vector3.Lerp(originalScale * startScaleFactor, originalScale, progress);
            yield return null;
        }

        if (enemy != null)
        {
            enemy.transform.localScale = originalScale;
            if (enemyCollider != null) enemyCollider.enabled = true;
        }
    }

    void SetThrustersEmission(float multiplier)
    {
        if (thrusterParticles == null) return;

        foreach (ParticleSystem ps in thrusterParticles)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.startSizeMultiplier = multiplier;
            }
        }
    }

    // --- Hjälpfunktioner för Unity Splines i World Space ---
    private Vector3 GetSplineWorldPosition(float t)
    {
        float3 localPos = splinePath.EvaluatePosition(t);
        return splinePath.transform.TransformPoint(localPos);
    }

    private Vector3 GetSplineWorldTangent(float t)
    {
        float3 localTangent = splinePath.EvaluateTangent(t);
        Vector3 worldTangent = splinePath.transform.TransformDirection(localTangent);
        return worldTangent.normalized;
    }
}