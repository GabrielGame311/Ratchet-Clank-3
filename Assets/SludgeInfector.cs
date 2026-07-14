using UnityEngine;

public class SludgeInfector : MonoBehaviour
{
    [Header("Damage & Range")]
    public float Damage = 10f;
    public float SearchRadius = 15f; // Hur långt bort den kan upptäcka fiender

    [Header("Movement Settings")]
    public float MoveSpeed = 6f;
    
    public float RotationSpeed = 10f; // Hur snabbt den svänger mot mål/riktning

    [Header("Physics & Grounding")]
    public LayerMask groundLayer;      // Välj din mark/terrain-layer här!
    public float groundOffset = 0.2f;  // Hur högt över markytan projektilens mittpunkt ska ligga

    [Header("Sway (No Target)")]
    public float swayAmount = 2f;      // Hur mycket den svänger i sidled
    public float swaySpeed = 3f;       // Hur snabbt den svajar fram och tillbaka

    [Header("Graphics")]
    public Transform visualModel;      // Dra in 3D-modellen (barnet) som ska rulla här

    private GameObject targetEnemy;
    private Vector3 currentVelocity;
    private float aliveTime;
    private Vector3 randomDirection;
    public Transform MeshFilter;


    [Header("Puddle Trail")]
    public GameObject puddlePrefab;    // Dra in din slem-pöl prefab här!
    public float distanceBetweenPuddles = 0.4f; // Hur ofta en pöl ska spawnas (i meter)
    private Vector3 lastPuddlePosition;

    void Start()
    {
        // Sätt en initial riktning framåt baserat på hur den sköts ut
        randomDirection = transform.forward;
        aliveTime = Random.Range(0f, 100f); // Slumpmässig start för sinusvågen så alla inte svajar likadant
        lastPuddlePosition = transform.position;
        if (visualModel == null)
        {
            // Om ingen modell har dragits in, försök hitta första child-objektet
            if (transform.childCount > 0)
                visualModel = transform.GetChild(0);
            else
                visualModel = transform;
        }

        // Förstör efter 10 sekunder om den inte träffar något
        Destroy(gameObject, 10f);
    }

    private void LateUpdate()
    {
        MeshFilter.transform.Rotate(-RotationSpeed * Time.deltaTime, 0, 0);
    }

    void Update()
    {

        // Kolla om vi har rört oss tillräckligt långt för att lägga en ny slem-pöl på marken
        float distanceMoved = Vector3.Distance(transform.position, lastPuddlePosition);
        if (distanceMoved >= distanceBetweenPuddles)
        {
           SpawnPuddle();
        }

        aliveTime += Time.deltaTime;

        // 1. SÖK EFTER FIENDE (om vi inte redan har en)
        if (targetEnemy == null)
        {
            FindNearestEnemy();
        }

        // 2. BERÄKNA RÖRELSERIKTNING (XZ-planet)
        Vector3 moveDirection = Vector3.zero;

        if (targetEnemy != null)
        {
            // Sväng mot fienden
            Vector3 toEnemy = (targetEnemy.transform.position - transform.position);
            toEnemy.y = 0; // Håll rörelsen platt på XZ
            moveDirection = toEnemy.normalized;
        }
        else
        {
            // Rulla framåt med ett mjukt svajande mönster i sidled
            Vector3 forward = randomDirection;
            Vector3 right = Vector3.Cross(Vector3.up, forward);

            // Skapar en mjuk sinus-kurva för zick-zack-rörelsen
            float sway = Mathf.Sin(aliveTime * swaySpeed) * swayAmount;
            moveDirection = (forward + right * sway).normalized;
        }

        // 3. APPLICERA RÖRELSE (Utmed marken)
        if (moveDirection != Vector3.zero)
        {
            // Flytta objektet framåt i rörelseriktningen
            transform.position += moveDirection * MoveSpeed * Time.deltaTime;

            // Rotera huvudobjektet mjukt i rörelseriktningen runt Y-axeln
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            // transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
            
        }
        
        // 4. KLISTRA OCH UTGÅ FRÅN MARKENS LUTNING (Raycasting)
        AlignWithGround();

        // 5. VISUELL RULL-ANIMATION
        // Rulla modellen framåt runt sin lokala X-axel
        
    }


    void SpawnPuddle()
    {
        if (puddlePrefab != null)
        {
            // Vi spawnar pölen precis vid bollens fötter/marken
            Vector3 spawnPos = transform.position;

            // Använd samma rotation som bollen har mot marken så att pölen ligger platt mot backen
            Instantiate(puddlePrefab, spawnPos, transform.rotation);

            lastPuddlePosition = transform.position;
        }
    }

    // Anpassar projektilen efter markens höjd och lutning
    void AlignWithGround()
    {
        RaycastHit hit;
        // Skjut en raycast från en bit ovanför projektilen och neråt
        Vector3 rayStart = transform.position + Vector3.up * 2f;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 5f, groundLayer))
        {
            // Sätt höjden exakt på marken plus din offset
            Vector3 newPosition = transform.position;
            newPosition.y = hit.point.y + groundOffset;
            transform.position = newPosition;

            // Rotera projektilen så att den lutar med marken (använd markens normal)
            Quaternion slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotation, 15f * Time.deltaTime);
        }
    }

    // Letar efter den närmaste fienden inom sökradien
    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");
        float closestDistance = SearchRadius;
        GameObject closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDistance)
                    closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            targetEnemy = closestEnemy;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Träffar vi en fiende?
        if (collision.collider.CompareTag("Enemie"))
        {
            EnemiesHealth enemyHealth = collision.collider.GetComponent<EnemiesHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)Damage);

                // HÄR KAN DU TRIGGA GRÖN INFICERINGS-EFFEKT / PARTIKLAR!
            }

            Destroy(gameObject);
        }

        SpawnPuddle();
    }
}