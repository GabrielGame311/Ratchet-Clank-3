using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerCover : MonoBehaviour
{
    [Header("Health & Combat")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    public float damagePerShot = 15f;
    
    [Tooltip("Välj vilket lager (t.ex. EnemyProjectiles / Explosions) som ska skada skyddet")]
    public LayerMask enemyLayerMask;

    [Header("Structure Setup")]
    [Tooltip("Övre metallbågen (Mesh0). Den har ALDRIG Rigidbody och försvinner direkt vid 0 HP.")]
    public Transform topMesh;

    [Header("Ratchet & Clank FX")]
    public GameObject breakParticlePrefab;
    public GameObject explosionParticlePrefab;
    public GameObject boltPrefab;
    public int boltsPerPiece = 3;
    public float boltScatterForce = 5f;
    public AudioClip hitSound;
    public AudioClip breakSound;
    public AudioClip destroySound;

    [Header("Physics Settings (Side Panels Only)")]
    public float pieceEjectForce = 8f;
    public float debrisLifetime = 5f;
    public float shrinkSpeed = 3f;

    private List<Transform> coverPieces = new List<Transform>();
    private List<MeshRenderer> pieceRenderers = new List<MeshRenderer>();
    private int initialPieceCount;
    private bool isFlashing = false;
    private bool isDestroyed = false;
    private MaterialPropertyBlock colorPropertyBlock;
    private AudioSource audioSource;

    void Awake()
    {
        currentHealth = maxHealth;
        colorPropertyBlock = new MaterialPropertyBlock();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Registrera alla paneler
        foreach (Transform child in transform)
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();

            // Hantera toppramen (topMesh får ALDRIG Rigidbody)
            if (topMesh != null && child == topMesh)
            {
                Rigidbody existingRb = child.GetComponent<Rigidbody>();
                if (existingRb != null)
                {
                    Destroy(existingRb); // Ta bort Rigidbody om den råkade finnas på objektet
                }

                if (renderer != null) pieceRenderers.Add(renderer);
                CoverPieceDamageProxy topProxy = child.gameObject.AddComponent<CoverPieceDamageProxy>();
                topProxy.Setup(this);
                continue;
            }

            if (renderer != null)
            {
                coverPieces.Add(child);
                pieceRenderers.Add(renderer);
                
                CoverPieceDamageProxy proxy = child.gameObject.AddComponent<CoverPieceDamageProxy>();
                proxy.Setup(this);
            }
        }
        
        initialPieceCount = coverPieces.Count;
    }

    public void TakeDamage(float damage, Vector3 hitPoint = default)
    {
        if (isDestroyed || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (hitSound != null) audioSource.PlayOneShot(hitSound);
        if (!isFlashing) StartCoroutine(FlashWhiteRoutine());

        // HP når 0: Förstör hela skyddet
        if (currentHealth <= 0)
        {
            isDestroyed = true;
            if (destroySound != null) audioSource.PlayOneShot(destroySound);
            DestroyCoverRatchetStyle(hitPoint);
            Destroy(gameObject);
            return;
        }

        // Räkna ut hur många sidopaneler som ska kopplas loss
        float healthPercentage = currentHealth / maxHealth;
        int targetPieceCount = Mathf.CeilToInt(healthPercentage * initialPieceCount);

        while (coverPieces.Count > targetPieceCount && coverPieces.Count > 0)
        {
            int randomIndex = Random.Range(0, coverPieces.Count);
            Transform pieceToBreak = coverPieces[randomIndex];
            
            if (pieceToBreak == null)
            {
                coverPieces.RemoveAt(randomIndex);
                continue;
            }

            MeshRenderer ren = pieceToBreak.GetComponent<MeshRenderer>();
            if (ren != null) pieceRenderers.Remove(ren);
            
            coverPieces.RemoveAt(randomIndex);

            var proxy = pieceToBreak.GetComponent<CoverPieceDamageProxy>();
            if (proxy != null) DestroyImmediate(proxy);

            if (breakSound != null) audioSource.PlayOneShot(breakSound);
            EjectPanel(pieceToBreak, hitPoint);
        }
    }

    private void EjectPanel(Transform panel, Vector3 hitPoint)
    {
        panel.SetParent(null); // Koppla loss helt från skyddet

        // Se till att den har collider
        MeshCollider collider = panel.gameObject.GetComponent<MeshCollider>();
        if (collider == null) collider = panel.gameObject.AddComponent<MeshCollider>();
        collider.convex = true;

        // Se till att den har Rigidbody
        Rigidbody rb = panel.gameObject.GetComponent<Rigidbody>();
        if (rb == null) rb = panel.gameObject.AddComponent<Rigidbody>();
        
        rb.isKinematic = false;
        rb.useGravity = true;

        // 1. RÄKNA UT RIKTNING UTÅT FRÅN MIDTEN AV SKYDDET
        Vector3 centerToPanel = (panel.position - transform.position).normalized; 
        centerToPanel.y = 0; // Håll horisontell kraft ren

        Vector3 hitToPanel = Vector3.zero;
        if (hitPoint != Vector3.zero)
        {
            hitToPanel = (panel.position - hitPoint).normalized;
        }

        // Kombinera: Utåt från mitten + bort från skottet + ETT REJÄLT LYFT UPPÅT
        Vector3 finalDirection = (centerToPanel * 0.7f + hitToPanel * 0.3f + Vector3.up * 0.8f).normalized;

        // 2. NOLLSTÄLL GAMMAL HASTIGHET FÖR REN SKJUTKRAFT
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 3. SKJUT IVÄG (ANVÄND IMPULSE)
        rb.AddForce(finalDirection * pieceEjectForce, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * pieceEjectForce * 2.5f, ForceMode.Impulse);

        SpawnBolts(panel.position);
        StartCoroutine(CleanUpDebris(panel));
    }

    private void DestroyCoverRatchetStyle(Vector3 epicenter)
    {
        // 1. Spawna explosionseffekt
        if (explosionParticlePrefab != null)
        {
            Instantiate(explosionParticlePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        // 2. Slungar ut alla kvarvarande sidopaneler med fysik
        for (int i = coverPieces.Count - 1; i >= 0; i--)
        {
            Transform piece = coverPieces[i];
            if (piece != null)
            {
                MeshRenderer ren = piece.GetComponent<MeshRenderer>();
                if (ren != null) pieceRenderers.Remove(ren);

                var proxy = piece.GetComponent<CoverPieceDamageProxy>();
                if (proxy != null) DestroyImmediate(proxy);

                EjectPanel(piece, epicenter);
            }
        }
        coverPieces.Clear();

        // 3. Ta bort topMesh helt UTAN Rigidbody eller fysik
        if (topMesh != null)
        {
            if (breakParticlePrefab != null)
            {
                Instantiate(breakParticlePrefab, topMesh.position, Quaternion.identity);
            }

            Destroy(topMesh.gameObject);
            topMesh = null;
        }
    }

    private void SpawnBolts(Vector3 position)
    {
        if (boltPrefab == null) return;

        for (int i = 0; i < boltsPerPiece; i++)
        {
            GameObject bolt = Instantiate(boltPrefab, position, Quaternion.identity);
            Rigidbody boltRb = bolt.GetComponent<Rigidbody>();
            if (boltRb != null)
            {
                Vector3 boltDir = new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 2f), Random.Range(-1f, 1f)).normalized;
                boltRb.AddForce(boltDir * boltScatterForce, ForceMode.Impulse);
            }
        }
    }

    private IEnumerator CleanUpDebris(Transform debris)
    {
        yield return new WaitForSeconds(debrisLifetime);

        if (debris == null) yield break;

        while (debris != null && debris.localScale.x > 0.05f)
        {
            debris.localScale = Vector3.MoveTowards(debris.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);
            yield return null;
        }

        if (breakParticlePrefab != null && debris != null)
        {
            Instantiate(breakParticlePrefab, debris.position, Quaternion.identity);
        }

        if (debris != null) Destroy(debris.gameObject);
    }

    private IEnumerator FlashWhiteRoutine()
    {
        isFlashing = true;

        colorPropertyBlock.SetColor("_BaseColor", Color.white);
        colorPropertyBlock.SetColor("_Color", Color.white);
        
        foreach (MeshRenderer ren in pieceRenderers)
        {
            if (ren != null) ren.SetPropertyBlock(colorPropertyBlock);
        }

        yield return new WaitForSeconds(0.06f);

        colorPropertyBlock.Clear();
        foreach (MeshRenderer ren in pieceRenderers)
        {
            if (ren != null) ren.SetPropertyBlock(colorPropertyBlock);
        }

        isFlashing = false;
    }
}

public class CoverPieceDamageProxy : MonoBehaviour
{
    private RangerCover mainCover;
    private bool hasTriggered = false;

    public void Setup(RangerCover masterScript)
    {
        mainCover = masterScript;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
        HandleHit(collision.gameObject, hitPoint);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject, other.transform.position);
    }

    private void HandleHit(GameObject hitObject, Vector3 hitPoint)
    {
        if (hasTriggered || mainCover == null) return;

        if ((mainCover.enemyLayerMask.value & (1 << hitObject.layer)) > 0)
        {
            hasTriggered = true; 
            mainCover.TakeDamage(mainCover.damagePerShot, hitPoint);
            Destroy(hitObject);
        }
    }
}