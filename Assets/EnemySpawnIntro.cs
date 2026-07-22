using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawnIntro : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject spawnParticlePrefab;
    public AudioClip spawnSound;
    public float spawnDelay = 0.5f;

    [Header("Spline Movement Settings")]
    [Tooltip("Dra in din SplineContainer från scenen som fienden ska följa")]
    public SplineContainer splinePath;
    public float moveSpeed = 6f;
    [Tooltip("Om fienden ska rotera i banans riktning när den springer")]
    public bool alignToPath = true;

    [Header("Fiendens Egna Skript")]
    [Tooltip("Dra in fiendens vanliga combat-skript här (slås på när fienden gått klart splinen)")]
    public MonoBehaviour combatScript;

    private Renderer[] enemyRenderers;
    private Animator animator;
    private AudioSource audioSource;
    private bool isMoving = false;
    private float progress = 0f; // Värde mellan 0 (start) och 1 (slut)
    private float splineLength = 0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        enemyRenderers = GetComponentsInChildren<Renderer>();
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        // Stäng av fiendens vanliga skript under intro-sekvensen
        if (combatScript != null) combatScript.enabled = false;

        if (splinePath != null)
        {
            splineLength = splinePath.CalculateLength();
        }

        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        isMoving = false;
        progress = 0f;

        // 1. Dölj fienden
        SetRenderersVisible(false);

        // 2. Skapa spawn-partikel och spela ljud
        if (spawnParticlePrefab != null)
        {
            GameObject fx = Instantiate(spawnParticlePrefab, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }

        // Vänta medan partikeln spelas
        yield return new WaitForSeconds(spawnDelay);

        // Om en spline finns, flytta fienden till splinens startposition direkt när den dyker upp
        if (splinePath != null)
        {
            UpdateSplinePositionAndRotation(0f);
        }

        // 3. Visa fienden och börja springa
        SetRenderersVisible(true);
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving || splinePath == null || splineLength <= 0) return;

        // Beräkna hur långt längs splinen fienden har kommit (0.0 till 1.0)
        progress += (moveSpeed / splineLength) * Time.deltaTime;

        if (progress >= 1f)
        {
            progress = 1f;
            UpdateSplinePositionAndRotation(1f);
            FinishIntro();
            return;
        }

        UpdateSplinePositionAndRotation(progress);

        if (animator != null) animator.SetBool("Run", true);
    }

    void UpdateSplinePositionAndRotation(float t)
    {
        // Hämtar exakt position och tangent (riktning) på splinen vid tidpunkten t
        Vector3 position = splinePath.EvaluatePosition(t);
        Vector3 tangent = splinePath.EvaluateTangent(t);

        transform.position = position;

        if (alignToPath && tangent != Vector3.zero)
        {
            tangent.y = 0; // Håll fienden rak horisontellt
            if (tangent != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
            }
        }
    }

    void FinishIntro()
    {
        isMoving = false;

        if (animator != null) animator.SetBool("Run", false);

        // 4. Aktivera fiendens egna skript när den nått slutet av splinen!
        if (combatScript != null)
        {
            combatScript.enabled = true;
        }

        // Stäng av intro-skriptet så det inte ligger och kör i bakgrunden
        this.enabled = false;
    }

    void SetRenderersVisible(bool visible)
    {
        foreach (Renderer r in enemyRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }
}