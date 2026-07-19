using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SludgeInfector : MonoBehaviour
{
    [Header("Damage & Range")]
    public float Damage = 10f;
    public float SearchRadius = 25f;

    [Header("Utskjutning & Fart")]
    public float LaunchForce = 4f;       
    public float LaunchUpwardForce = 1.5f; 
    public float MoveSpeed = 6f;         

    [Header("Pingis-studs (Tung Gravitation)")]
    public float GravityMultiplier = 7f; 

    [Header("Graphics")]
    public Transform visualModel;      
    public Transform MeshFilter;
    public float RotationSpeed = 300f; 
    
    private float currentRollAngle = 0f; 
    private Vector3 currentMoveDirection; // Håller reda på den aktuella raka riktningen
    private Rigidbody rb;

    [Header("Puddle Trail")]
    public GameObject puddlePrefab;    
    public float distanceBetweenPuddles = 0.4f; 
    private Vector3 lastPuddlePosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.isKinematic = false;
        rb.useGravity = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Startriktningen är rakt framåt från där den skjuts ut
        currentMoveDirection = transform.forward;
        currentMoveDirection.y = 0;
        currentMoveDirection.Normalize();

        // Startskottet
        Vector3 launchDir = (currentMoveDirection * LaunchForce) + (Vector3.up * LaunchUpwardForce);
        rb.AddForce(launchDir, ForceMode.Impulse);

        lastPuddlePosition = transform.position;

        if (visualModel == null)
        {
            if (transform.childCount > 0)
                visualModel = transform.GetChild(0);
            else
                visualModel = transform;
        }

        Destroy(gameObject, 10f);
    }

    void Update()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPuddlePosition);
        if (distanceMoved >= distanceBetweenPuddles)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f))
            {
                SpawnPuddle(hit.point, hit.normal);
            }
        }

       
    }

    void FixedUpdate()
    {
        // Tung gravitation drar ner bollen för pingis-effekten
        rb.AddForce(Physics.gravity * GravityMultiplier, ForceMode.Acceleration);

        // Håll farten stabil framåt i den riktning som är vald just nu (svänger INTE i luften)
        Vector3 currentXZVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        Vector3 targetXZVelocity = currentMoveDirection * MoveSpeed;
        
        Vector3 velocityChange = targetXZVelocity - currentXZVelocity;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        // Rotera baserat på färdriktning
        if (currentMoveDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(currentMoveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 15f * Time.fixedDeltaTime);
        }
    }

    private void LateUpdate()
    {
        if (MeshFilter != null)
        {
            // 1. Kaotisk rotation (Tumla)
            MeshFilter.transform.Rotate(new Vector3(1f, 0.7f, 0.4f) * RotationSpeed * Time.deltaTime, Space.Self);

            // 2. Wobble-effekt (Squash & Stretch)
            // Får slembollen att snabbt krympa och växa pyttelite så den ser "slajmig" ut
            float wobble = 1f + Mathf.Sin(Time.time * 15f) * 0.15f; 
            MeshFilter.transform.localScale = new Vector3(wobble, 2f - wobble, wobble);
        }
    }

    private void OnCollisionEnter(Collision collision)
{
    // 1. Om vi krockar direkt med en fiende
    if (collision.collider.CompareTag("Enemie"))
    {
        EnemiesHealth enemyHealth = collision.collider.GetComponent<EnemiesHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage((int)Damage);
            enemyHealth.Infect(true); 
        }

        SpawnPuddle(collision.contacts[0].point, collision.contacts[0].normal);
        Destroy(gameObject);
        return;
    }

    // Hämta normalen på ytan vi krockade med för att veta om det är vägg eller golv
    Vector3 hitNormal = collision.contacts[0].normal;
    
    // Om Y-värdet på normalen är lågt (närmare 0), betyder det att ytan är lodrät = en VÄGG
    bool isWall = Mathf.Abs(hitNormal.y) < 0.5f;

    SpawnPuddle(collision.contacts[0].point, hitNormal);

    GameObject nearestEnemy = FindNearestEnemy();

    // 2. Om vi krockade med en VÄGG
    if (isWall)
    {
        if (nearestEnemy != null)
        {
            // Gå in mot fienden
            Vector3 toEnemy = (nearestEnemy.transform.position - transform.position);
            toEnemy.y = 0;
            currentMoveDirection = toEnemy.normalized;
        }
        else
        {
            // Slumpa 90 grader vänster eller höger (eftersom vi slog i en vägg och ingen fiende finns)
            if (Random.value > 0.5f)
            {
                currentMoveDirection = new Vector3(currentMoveDirection.z, 0, -currentMoveDirection.x).normalized;
            }
            else
            {
                currentMoveDirection = new Vector3(-currentMoveDirection.z, 0, currentMoveDirection.x).normalized;
            }
        }
    }
    // 3. Om vi krockade med GOLVET (studs)
    else
    {
        // Ändra bara riktning om vi har en fiende att jaga, annars fortsätt rakt fram (studsa vidare)
        if (nearestEnemy != null)
        {
            Vector3 toEnemy = (nearestEnemy.transform.position - transform.position);
            toEnemy.y = 0;
            currentMoveDirection = toEnemy.normalized;
        }
    }
}

    void SpawnPuddle(Vector3 spawnPos, Vector3 groundNormal)
    {
        if (puddlePrefab != null)
        {
            Quaternion puddleRot = Quaternion.FromToRotation(Vector3.up, groundNormal);
            Instantiate(puddlePrefab, spawnPos + groundNormal * 0.02f, puddleRot);
            lastPuddlePosition = transform.position;
        }
    }

    GameObject FindNearestEnemy()
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
                {
                    closestDistance = dist;
                    closestEnemy = enemy;
                }
            }
        }
        return closestEnemy;
    }
}