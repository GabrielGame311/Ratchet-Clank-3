using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniThyrra : MonoBehaviour
{
    [Header("Combat")]
    public int damage = 10;
    public float attackInterval = 1f;   // hur ofta den får slå
    public float attackRange = 1.2f;    // DistanceDamage

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 0.5f;   // hur nära den stannar

    [Header("Detection")]
    public LayerMask playerLayer;

    private Animator anim;
    
    // Används nu dynamiskt via EnemiesHealth istället för att bara söka efter "Player"
    private Transform currentTarget; 
    private EnemiesHealth myHealth;

    public float attackTimer;

    bool isJumping = false;
    bool IsPatroling = false;
    public float patrolSpeed = 2f;
    public float patrolSwitchTime = 3f;

    private float patrolTimer;
    private int patrolDirection = 1; // 1 = framåt, -1 = bakåt
    public float turnDuration = 0.5f; // Hur lång tid rotationen ska ta

    public float Jump;
    bool isPatrolWaiting = false;

    Rigidbody rb;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        myHealth = GetComponent<EnemiesHealth>();
        
        patrolTimer = patrolSwitchTime;
        IsPatroling = true;
    }

    void Update()
    {
        if (isJumping) return;

        // Hämta det aktuella målet direkt från EnemiesHealth!
        currentTarget = (myHealth != null) ? myHealth.currentTarget : null;

        if (currentTarget == null)
        {
            Patrol();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        if (dist > attackRange && dist > 10f)
        {
            Patrol();
            return;
        }

        if (dist <= attackRange || dist <= 10f)
        {
            anim.SetBool("Walk", false); // 🔁 Slå av patrullanimation
            IsPatroling = false;
            ChaseOrAttack(dist);
            return;
        }
    }

    void ChaseOrAttack(float dis)
    {
        if (isJumping) return;

        if (currentTarget == null) { anim.SetBool("Run", false); return; }

        float dist = Vector3.Distance(transform.position, currentTarget.position);

        Vector3 lookDir = (currentTarget.position - transform.position); lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        if (dist > attackRange)
        {
            anim.SetBool("Run", true);
            rb.isKinematic = false;
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            anim.SetBool("Run", false);
            attackTimer -= Time.deltaTime;
            rb.isKinematic = true;

            if (attackTimer <= 0f)
            {
                attackTimer = attackInterval;
                anim.SetTrigger("Attack");
                
                // Deal Damage baserat på vem målet är
                DealDamageToTarget();

                // Starta bakåt-hopp
                StartCoroutine(BackJump());
            }
        }
    }

    void DealDamageToTarget()
    {
        if (currentTarget == null) return;

        if (currentTarget.CompareTag("Player"))
        {
            Player playerComponent = currentTarget.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.TakeDamage(damage);
            }
        }
        else if (currentTarget.CompareTag("Enemie"))
        {
            EnemiesHealth otherEnemy = currentTarget.GetComponent<EnemiesHealth>();
            if (otherEnemy != null)
            {
                otherEnemy.TakeDamage(damage);
            }
        }
    }

    void Patrol()
    {
        if (isPatrolWaiting) return; // 🛑 pausera rörelse
        anim.SetBool("Run", false);
        anim.SetBool("Walk", true);
        IsPatroling = true;
        rb.isKinematic = false;

        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
        {
            StartCoroutine(PatrolPause()); // 👉 starta paus
            return;
        }

        // Gå framåt
        Vector3 dir = new Vector3(0, 0, patrolDirection);
        transform.position += dir * patrolSpeed * Time.deltaTime;

        transform.rotation = Quaternion.LookRotation(dir);
    }

    IEnumerator PatrolPause()
    {
        isPatrolWaiting = true;
        anim.SetBool("Walk", false);
        rb.linearVelocity = Vector3.zero;

        // Vänta stilla först
        yield return new WaitForSeconds(3.5f);

        // Mjuk vändning
        Quaternion startRot = transform.rotation;
        Vector3 newDir = new Vector3(0, 0, -patrolDirection); // Ny riktning
        Quaternion targetRot = Quaternion.LookRotation(newDir);

        float elapsed = 0f;
        while (elapsed < turnDuration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / turnDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Säkerställ exakt rotation
        transform.rotation = targetRot;

        // Starta patrull igen
        patrolDirection *= -1;
        patrolTimer = patrolSwitchTime;
        anim.SetBool("Walk", true);
        isPatrolWaiting = false;
    }

    IEnumerator BackJump()
    {
        yield return new WaitForSeconds(0.5f);
        isJumping = true;

        // Hoppa bakåt – baserat på framåtriktning
        Vector3 jumpDirection = -transform.forward + Vector3.up * 0.5f;
        rb.isKinematic = false;
        rb.AddForce(jumpDirection.normalized * Jump, ForceMode.Impulse);
        anim.SetTrigger("Jump");

        yield return new WaitForSeconds(1.0f); // vänta under hopp

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        yield return new WaitForSeconds(0.5f); // kort vila efter hopp

        isJumping = false;
    }

    // Eftersom det nya systemet sköter målsökningen i bakgrunden via krockar och avstånd,
    // kan vi städa bort de gamla OnTrigger-metoderna om du vill, men de kan ligga kvar som backup
    // ifall du vill sätta ett primärt mål manuellt vid krock.
   private void OnTriggerEnter(Collider other)
    {
        // Om vi krockar med spelaren, berätta för EnemiesHealth att detta är vårt mål!
        if (other.CompareTag("Player"))
        {
            if (myHealth != null)
            {
                myHealth.currentTarget = other.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Om spelaren springer utanför vår trigger, tappar vi målet och börjar patrullera igen
        if (other.CompareTag("Player"))
        {
            if (myHealth != null && myHealth.currentTarget == other.transform)
            {
                myHealth.currentTarget = null;
            }
        }
    }
}