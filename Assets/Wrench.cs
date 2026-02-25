using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wrench : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform handPosition;
    public Transform Object; // Själva rörtångs-modellen
    public RatchetController controller;
    public Animator anime; // Ratchet Animator
    public Animator animeWrench; // Wrench Animator
    public AudioSource wrnechsound;
    public ParticleSystem particleSplash;
    public LayerMask enemyLayer;

    [Header("Melee Settings")]
    public float moveSpeed = 10f;
    public float attackMoveDistance = 2f;
    public float autoAimRadius = 5f;
    public float autoAimAngle = 60f;
    public float lungeDistance = 1.5f;
    public int damage = 1;

    [Header("Throw Settings")]
    public float throwSpeed = 20f;
    public float returnDelay = 0.3f;
    public float throwDuration = 0.6f;

    // Privata kontrollvariabler
    public bool Isplaying { get; private set; } // Används för Trigger-logik
    private bool isMoving = false;
    private bool isThrowing = false;
    private Transform weaponParent;
    private Vector3 targetPosition;

    private void Start()
    {
        // Spara föräldern (WeaponSwitcher) så vi kan sätta tillbaka den efter kast
        weaponParent = transform.parent;

        if (anime == null)
            anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();

        if (controller == null)
            controller = GameObject.FindObjectOfType<RatchetController>();
    }

    private void Update()
    {
        // Sätt animation-state
        anime.SetBool("Gun", false);

        if (gameObject.activeSelf)
        {
            // Attack (Melee)
            if (Input.GetKeyDown(KeyCode.F) && !Isplaying && !isThrowing)
            {
                StartMeleeAttack();
            }

            // Kast (Wrench Throw)
            if (Input.GetKeyDown(KeyCode.Mouse1) && !isThrowing && !Isplaying)
            {
                StartCoroutine(ThrowWrenchRoutine());
            }

            // Hantera lunge-förflyttning
            if (isMoving)
            {
                MoveForward();
            }
        }
    }

    // --- MELEE LOGIK ---

    void StartMeleeAttack()
    {
        Isplaying = true; // Nu kan vi göra skada via OnTriggerEnter
        if (wrnechsound) wrnechsound.Play();

        // 1. Auto-aim: Titta på närmsta fiende
        TryAutoTarget();

        // 2. Starta animationer
        anime.SetTrigger("Wrench");
        if (animeWrench) animeWrench.SetTrigger("Hit");

        // 3. Starta Lunge (hoppet framåt)
        targetPosition = player.position + player.forward * attackMoveDistance;
        isMoving = true;

        // 4. Stäng av skadan efter en kort stund (när svingen är klar)
        Invoke("EndAttack", 0.6f);
    }

    void TryAutoTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(player.position, autoAimRadius, enemyLayer);
        Transform closestEnemy = null;
        float closestAngle = autoAimAngle;

        foreach (var enemy in enemies)
        {
            Vector3 dirToEnemy = (enemy.transform.position - player.position).normalized;
            float angle = Vector3.Angle(player.forward, dirToEnemy);

            if (angle < closestAngle)
            {
                closestAngle = angle;
                closestEnemy = enemy.transform;
            }
        }

        if (closestEnemy != null)
        {
            Vector3 lookDir = (closestEnemy.position - player.position).normalized;
            lookDir.y = 0;
            player.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    void MoveForward()
    {
        Vector3 moveStep = player.forward * moveSpeed * Time.deltaTime;
        controller.MyController.Move(moveStep);

        // Stoppa om vi nått målet eller tiden ute
        if (Vector3.Distance(player.position, targetPosition) < 0.2f)
        {
            isMoving = false;
        }
    }

    void EndAttack()
    {
        Isplaying = false;
        isMoving = false;
    }

    // --- THROW LOGIK ---


    public void Throw1()
    {
        StartCoroutine(ThrowWrenchRoutine());
    }


    private IEnumerator ThrowWrenchRoutine()
    {
        isThrowing = true;
        Isplaying = true;

        // Nollställ triggern först för att vara säker på att den inte "köar"
        anime.ResetTrigger("ThrowWrench");
        anime.SetTrigger("ThrowWrench");

        transform.SetParent(null);

        // Ut-kast
        float timer = 0f;
        while (timer < throwDuration)
        {
            transform.position += transform.forward * throwSpeed * Time.deltaTime;
            Object.Rotate(0f, 1000f * Time.deltaTime, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(returnDelay);

        // Hem-väg (Vi lägger till en säkerhetstimer här så den inte loopar för evigt)
        float returnTimer = 0f;
        while (Vector3.Distance(transform.position, handPosition.position) > 0.6f && returnTimer < 3f)
        {
            transform.position = Vector3.MoveTowards(transform.position, handPosition.position, throwSpeed * Time.deltaTime);
            Object.Rotate(0f, 1000f * Time.deltaTime, 0f);
            returnTimer += Time.deltaTime; // Om den inte hittar hem på 3 sekunder, avbryt
            yield return null;
        }

        // Återställ allt
        transform.SetParent(weaponParent);
        transform.localPosition = Vector3.zero; // Använd nollställd lokal position
        transform.localRotation = Quaternion.identity;
        Object.localRotation = Quaternion.identity;

        isThrowing = false;
        Isplaying = false;
    }

    // --- COLLISION LOGIK ---

    private void OnTriggerEnter(Collider other)
    {
        // Kolla om rörtången är "aktiv" (under sving eller kast)
        if (!Isplaying) return;

        // Här använder vi "TryGetComponent" för att slippa errors om script saknas
        if (other.CompareTag("Box"))
        {
            if (other.TryGetComponent(out Box box)) box.Break(damage);
            SpawnEffect();
        }
        else if (other.CompareTag("Enemie"))
        {
            if (other.TryGetComponent(out EnemiesHealth enemy)) enemy.TakeDamage(damage);
            SpawnEffect();
        }
        else if (other.CompareTag("Health"))
        {
            if (other.TryGetComponent(out HealthItem health)) health.Break(damage);
        }
        else if (other.CompareTag("AmmoBox"))
        {
            if (other.TryGetComponent(out AmmoCrate ammo)) ammo.Break(damage);
        }
    }

    void SpawnEffect()
    {
        if (particleSplash != null)
        {
            particleSplash.Play();
        }
        // Här kan du lägga till Isplaying = false om du bara vill träffa ETT objekt per slag
    }
}