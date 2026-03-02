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
    private Transform player;
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
        patrolTimer = patrolSwitchTime;
        IsPatroling = true;
    }


  
        void Update()
        {
            if (isJumping) return;

            if (player == null)
            {
                Patrol();
                return;
            }

            float dist = Vector3.Distance(transform.position, player.position);

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

        if (player == null) { anim.SetBool("Run", false); return; }

        float dist = Vector3.Distance(transform.position, player.position);

        Vector3 lookDir = (player.position - transform.position); lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        if (dist > attackRange)
        {
            anim.SetBool("Run", true);
            rb.isKinematic = false;
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
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
                player.GetComponent<Player>().TakeDamage(damage);

                // Starta bakåt-hopp
                StartCoroutine(BackJump());
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
       // anim.SetBool("Walk", true);
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

        // Vänta stilla efter vändning
        //yield return new WaitForSeconds(2);

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


    // Trigger för att börja följa spelaren
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            player = other.transform;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            player = null;
    }
}
