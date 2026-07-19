using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothDropship : MonoBehaviour
{
    public enum FinalTurn { Left, Right }

    [Header("Waypoints")]
    [Tooltip("Där skeppet ska stanna och släppa av fiender")]
    public Transform dropPoint;
    [Tooltip("En valfri punkt på vägen ut, eller lämna tom för att flyga rakt ut i rymden")]
    public Transform exitWayPoint;

    [Header("R&C 3 Exit Settings")]
    public FinalTurn finalTurnDirection = FinalTurn.Right;
    public float finalExitDistance = 250f;
    [Tooltip("Hur snabbt skeppet accelererar när det flyr")]
    public float accelerationRate = 55f; 
    [Tooltip("Toppfarten när skeppet dundrar iväg")]
    public float maxExitSpeed = 95f;     

    [Header("Movement Settings")]
    public float entrySpeed = 35f;
    public float turnSpeed = 5f;
    [Tooltip("Hur mycket skeppet lutar i svängarna")]
    public float bankingAmount = 50f; 

    [Header("Braking & Weight (Overshoot)")]
    [Tooltip("Hur mycket skeppet glider förbi drop-punkten innan det fjädrar tillbaka (ger tyngd!)")]
    public float brakeOvershootAmount = 4f;
    [Tooltip("Hur snabbt skeppet fjädrar tillbaka efter inbromsningen")]
    public float brakeSettleSpeed = 3f;

    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public Transform spawnPoint;
    public float timeBetweenEnemies = 0.4f;
    [Tooltip("Kraften fienderna skjuts ut med (båge framåt/nedåt)")]
    public float enemyLaunchForce = 14f;

    [Header("Scaling & Spawn Fix Settings")]
    public float scaleUpDuration = 0.5f;
    public float startScaleFactor = 0.15f;

    [Header("VFX & Juice (Valfritt)")]
    [Tooltip("Partikelsystem för motorerna som blir större när skeppet gasar")]
    public ParticleSystem[] thrusterParticles;
    [Tooltip("Ljudkälla för motorvrål")]
    private AudioSource audioSource;

    private enum ShipState { Incoming, Braking, Dropping, Exiting, Finished }
    private ShipState currentState = ShipState.Incoming;
    private Animator anim;
    private Vector3 currentTarget;
    private float currentSpeed;
    private Vector3 overshootTarget;
    private float brakeTimer = 0f;

    private Collider[] shipColliders;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        currentSpeed = entrySpeed;

        shipColliders = GetComponentsInChildren<Collider>();

        if (dropPoint != null)
        {
            currentTarget = dropPoint.position;
        }
        else
        {
            Debug.LogError("Drop Point saknas på " + gameObject.name);
        }

        SetThrustersEmission(1f); // Normala motorpartiklar vid start
    }

    void Update()
    {
        switch (currentState)
        {
            case ShipState.Incoming:
            case ShipState.Exiting:
                MoveShip();
                break;

            case ShipState.Braking:
                HandleBraking();
                break;

            case ShipState.Dropping:
                ApplyHoverEffect();
                break;
        }
    }

    void MoveShip()
    {
        if (currentState == ShipState.Exiting)
        {
            // Öka farten extremt snabbt (Rocket Boost!)
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxExitSpeed, accelerationRate * Time.deltaTime);
            SetThrustersEmission(2.5f); // Gör eld-effekten mycket större!
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, currentSpeed * Time.deltaTime);

        Vector3 direction = (currentTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            float angleDiff = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
            float targetBank = Mathf.Clamp(angleDiff * 2.5f, -bankingAmount, bankingAmount);
            targetRot *= Quaternion.Euler(0, 0, -targetBank);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }

        // När vi är nära målet
        if (Vector3.Distance(transform.position, currentTarget) < 3f)
        {
            if (currentState == ShipState.Incoming)
            {
                // Istället för att bara stanna, påbörja den tunga inbromsningen (Overshoot)
                SetupBrakeOvershoot();
            }
            else
            {
                CheckNextState();
            }
        }
    }

    void SetupBrakeOvershoot()
    {
        currentState = ShipState.Braking;
        brakeTimer = 0f;
        
        // Räkna ut en punkt lite framför drop-punkten som skeppet glider förbi till
        Vector3 forwardDir = (dropPoint.position - transform.position).normalized;
        overshootTarget = dropPoint.position + (forwardDir * brakeOvershootAmount);
    }

    void HandleBraking()
    {
        brakeTimer += Time.deltaTime * brakeSettleSpeed;

        // Använd en Sinus-kurva för att mjukt glida förbi dropPoint och sedan fjädra tillbaka
        float t = Mathf.Sin(brakeTimer * Mathf.PI * 0.5f); // Går från 0 till 1
        
        if (brakeTimer < 1f)
        {
            // Glider framåt mot overshoot-punkten
            transform.position = Vector3.Lerp(transform.position, overshootTarget, t);
        }
        else
        {
            // Fjädrar tillbaka och landar perfekt på dropPoint
            transform.position = Vector3.Lerp(transform.position, dropPoint.position, (brakeTimer - 1f));
            
            if (brakeTimer >= 2f)
            {
                transform.position = dropPoint.position;
                StartCoroutine(DropSequence());
            }
        }

        // Mjuk broms-rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, dropPoint.rotation, Time.deltaTime * turnSpeed);
    }

    void CheckNextState()
    {
        if (currentState == ShipState.Exiting)
        {
            if (exitWayPoint != null && currentTarget == exitWayPoint.position)
            {
                CalculateEscapeVector();
            }
            else
            {
                currentState = ShipState.Finished;
                Destroy(gameObject);
            }
        }
    }

    void StartExitSequence()
    {
        currentState = ShipState.Exiting;
        
        if (exitWayPoint != null)
        {
            currentTarget = exitWayPoint.position;
        }
        else
        {
            CalculateEscapeVector();
        }

        currentSpeed += 15f; // Extra kick direkt!
    }

    void CalculateEscapeVector()
    {
        Vector3 sideDir = (finalTurnDirection == FinalTurn.Right) ? transform.right : -transform.right;
        Vector3 forwardDir = transform.forward;

        // Skjut skeppet snett framåt/sidan och uppåt i rymden
        Vector3 escapeDirection = (sideDir + forwardDir + Vector3.up * 0.4f).normalized;
        currentTarget = transform.position + escapeDirection * finalExitDistance;
    }

    void ApplyHoverEffect()
    {
        if (dropPoint == null) return;

        // Intensivt och "nervöst" hovrande
        float hover = Mathf.Sin(Time.time * 4.5f) * 0.35f; 
        transform.position = dropPoint.position + new Vector3(0, hover, 0);

        float roll = Mathf.Sin(Time.time * 3.5f) * 5f;
        float pitch = Mathf.Cos(Time.time * 3.0f) * 2.5f;
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            dropPoint.rotation * Quaternion.Euler(pitch, 0, roll), 
            Time.deltaTime * 6f
        );
    }

    IEnumerator DropSequence()
    {
        currentState = ShipState.Dropping;
        SetThrustersEmission(0.3f); // Minska eld-effekten under hovring
        
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

    IEnumerator ScaleAndLaunchEnemy(GameObject enemy)
    {
        if (enemy == null) yield break;

        Vector3 originalScale = enemy.transform.localScale;
        enemy.transform.localScale = originalScale * startScaleFactor;

        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        Collider enemyCollider = enemy.GetComponent<Collider>();

        // Stäng av kollisioner tillfälligt
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

        // Skjut ut fienden i en härlig fallande båge
        if (enemyRb != null)
        {
            enemyRb.velocity = Vector3.zero;
            Vector3 pushDir = (transform.forward * 1.9f + Vector3.down * 0.1f).normalized;
            enemyRb.AddForce(pushDir * enemyLaunchForce, ForceMode.Impulse);
        }

        // Mjuk uppskalning
        float timer = 0f;
        while (timer < scaleUpDuration)
        {
            if (enemy == null) yield break;

            timer += Time.deltaTime;
            float progress = timer / scaleUpDuration;
            enemy.transform.localScale = Vector3.Lerp(originalScale * startScaleFactor, originalScale, progress);
            
            yield return null;
        }

        // Aktivera kollisioner igen
        if (enemy != null)
        {
            enemy.transform.localScale = originalScale;
            if (enemyCollider != null) enemyCollider.enabled = true;
        }
    }

    // Ändrar partiklarnas storlek dynamiskt beroende på om vi bromsar, hovrar eller flyr!
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
}