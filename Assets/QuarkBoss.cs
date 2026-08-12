using System.Collections;
using UnityEngine;

public class QuarkBoss : MonoBehaviour
{
    [Header("Target & Movement")]
    public Transform player;
    public float runSpeed = 7f;
    public float rotateSpeed = 8f;

    [Header("Combat Ranges")]
    public float detectionRange = 30f;
    public float throwRange = 16f;
    public float meleeRange = 3f;

    [Header("Melee Settings")]
    public float meleeCooldown = 2.5f;
    private float lastMeleeTime;

    [Header("Weapon & Throw Settings")]
    public GameObject weaponInHand;
    public GameObject thrownWeaponPrefab;
    public Transform throwHand;
    public float throwCooldown = 4f;
    private float lastThrowTime;

    [Header("Phase: Hanging Mechanic")]
    [Tooltip("Dra in ett tomt GameObject placerat i luften där Qwark ska hänga.")]
    public Transform hangingPoint;

    [Tooltip("Procent hälsa då han hoppar upp och hänger (0.7 = 70%).")]
    public float hangAtHealthPercent = 0.7f;

    [Tooltip("Procent hälsa då han trillar ner igen efter att ha tagit skada (0.5 = 50%).")]
    public float dropAtHealthPercent = 0.5f;

    [Header("Components")]
    public Animator anime;
    private EnemiesHealth health;
    private Rigidbody rb;

    private float maxHealth;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isHanging = false;
    private bool hasHung = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<EnemiesHealth>();
        if (anime == null) anime = GetComponent<Animator>();

        if (health != null)
        {
            maxHealth = health.health;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (health != null && health.health <= 0)
        {
            Die();
            return;
        }

        if (player == null) return;

        // --- FAS-KONTROLL (70% HÄLSA MEKANIK) ---
        if (health != null && maxHealth > 0)
        {
            float healthPercent = health.health / maxHealth;

            // 1. Nått 70% HP -> Hoppa upp och häng i luften
            if (!hasHung && healthPercent <= hangAtHealthPercent)
            {
                StartCoroutine(JumpToHangRoutine());
                return;
            }

            // 2. Medan han hänger i luften -> Lås positionen och vänta!
            if (isHanging)
            {
                if (hangingPoint != null)
                {
                    transform.position = hangingPoint.position;
                }

                RotateTowardsPlayer(); // Titta mot spelaren medan han väntar

                // När spelaren skjutit honom ner till 50% HP -> Fall ner
                if (healthPercent <= dropAtHealthPercent)
                {
                    StartCoroutine(FallFromHangRoutine());
                }

                return; // Stoppa all annan attack- och rörelse-logik medan han hänger!
            }
        }

        // --- VANLIG MARKSTRID ---
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            if (isAttacking)
            {
                RotateTowardsPlayer();
                return;
            }

            bool throwReady = Time.time >= lastThrowTime + throwCooldown;
            bool meleeReady = Time.time >= lastMeleeTime + meleeCooldown;

            if (distance <= throwRange && throwReady)
            {
                if (distance <= meleeRange && meleeReady && Random.value < 0.5f)
                {
                    PerformMelee();
                }
                else
                {
                    PerformThrow();
                }
            }
            else if (distance <= meleeRange && meleeReady)
            {
                PerformMelee();
            }
            else if (distance > meleeRange)
            {
                RunTowardsPlayer();
            }
            else
            {
                StopRunning();
                RotateTowardsPlayer();
            }
        }
        else
        {
            StopRunning();
        }
    }

    // --- COROUTINES FÖR HÄNG OCH FALL ---

    private IEnumerator JumpToHangRoutine()
    {
        hasHung = true;
        isAttacking = true;
        StopRunning();

        // 1. Tänd Hang-animationen i Animatorn
        if (anime != null) anime.SetBool("Hang", true);

        Vector3 startPos = transform.position;
        Vector3 targetPos = (hangingPoint != null) ? hangingPoint.position : startPos + Vector3.up * 8f;

        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * 2f;
            transform.position = currentPos;

            yield return null;
        }

        transform.position = targetPos;
        isHanging = true;
        isAttacking = false;
    }

    private IEnumerator FallFromHangRoutine()
    {
        isHanging = false;
        isAttacking = true;

        // 2. Släck Hang-animationen så han spelar ExitHang och trillar ner
        if (anime != null) anime.SetBool("Hang", false);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos;

        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, 40f))
        {
            targetPos = hit.point;
        }
        else
        {
            targetPos.y -= 8f;
        }

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPos, targetPos, t * t);
            yield return null;
        }

        transform.position = targetPos;
        isAttacking = false;
    }

    // --- ATTACKER OCH HJÄLPMETODER ---

    void PerformMelee()
    {
        StopRunning();
        RotateTowardsPlayer();
        lastMeleeTime = Time.time;
        isAttacking = true;
        if (anime != null) anime.SetTrigger("Hit");

        Invoke(nameof(ResetAttack), 1.2f);
    }

    void PerformThrow()
    {
        StopRunning();
        RotateTowardsPlayer();
        lastThrowTime = Time.time;
        isAttacking = true;
        if (anime != null) anime.SetTrigger("Throw");

        Invoke(nameof(SpawnThrowObject), 0.35f);

        CancelInvoke(nameof(ForceResetWeapon));
        Invoke(nameof(ForceResetWeapon), 4.0f);
    }

    void RunTowardsPlayer()
    {
        if (anime != null) anime.SetBool("Run", true);
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, runSpeed * Time.deltaTime);
        RotateTowardsPlayer();
    }

    void StopRunning()
    {
        if (anime != null) anime.SetBool("Run", false);
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        }
    }

    public void SpawnThrowObject()
    {
        if (thrownWeaponPrefab == null || player == null) return;

        if (weaponInHand != null) weaponInHand.SetActive(false);

        Transform spawnPoint = (throwHand != null) ? throwHand : transform;
        Vector3 targetPos = player.position + Vector3.up * 1f;
        Vector3 directionToPlayer = (targetPos - spawnPoint.position).normalized;
        Quaternion spawnRotation = Quaternion.LookRotation(directionToPlayer);

        GameObject thrownObj = Instantiate(thrownWeaponPrefab, spawnPoint.position, spawnRotation);

        if (thrownObj.TryGetComponent<ReturningWeapon>(out ReturningWeapon weaponScript))
        {
            weaponScript.Initialize(targetPos, spawnPoint, this);
        }
    }

    public void OnWeaponReturned()
    {
        if (weaponInHand != null) weaponInHand.SetActive(true);
        isAttacking = false;
    }

    void ForceResetWeapon()
    {
        if (weaponInHand != null && !weaponInHand.activeSelf)
        {
            weaponInHand.SetActive(true);
            isAttacking = false;
        }
    }

    public void OnTakeDamage()
    {
        if (isDead) return;
        if (anime != null) anime.SetTrigger("Damage");
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void Die()
    {
        isDead = true;
        StopRunning();
        if (anime != null) anime.SetTrigger("Die");

        if (TryGetComponent<Collider>(out Collider col)) col.enabled = false;
        this.enabled = false;
    }
}