using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThyrranoidArcShip : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Transform Point1;
    public Transform Point2;
    public GameObject BulletPrefab;
    public float ShootForce = 70f;

    [Header("Flight - Player (Flying)")]
    public float playerOrbitRadius = 30f;   // Större cirkel för flygstrid
    public float playerFlyHeight = 2f;      // Låg höjdskillnad eftersom båda flyger

    [Header("Flight - Rangers (Ground)")]
    public float rangerOrbitRadius = 15f;   // Mindre cirkel för markmål
    public float rangerFlyHeight = 10f;     // Högre upp så de inte krockar med marken

    [Header("Global Flight Settings")]
    public float moveSpeed = 20f;
    public float rotationSpeed = 3.0f;      // Sänkt för att inte vara så "ryckig"
    public float orbitSpeedMultiplier = 0.5f; // Sänkt så den inte snurrar för snabbt
    public float bankingAmount = 55f;

    [Header("AI Logic")]
    public float detectionRange = 60f;
    public LayerMask playerLayer;
    public LayerMask rangerLayer;

    private GameObject currentTarget;
    private bool hasTarget = false;
    private bool targetIsPlayer = false;
    private float orbitTimer;
    private Animator anime;

    void Start()
    {
        anime = GetComponentInChildren<Animator>();
        orbitTimer = Random.Range(0f, 10f);
    }

    void Update()
    {
        ManageTargeting();

        if (hasTarget && currentTarget != null)
        {
            HandleCombatFlight();
        }
        else
        {
            transform.position += transform.forward * (moveSpeed * 0.4f) * Time.deltaTime;
        }
    }

    void ManageTargeting()
    {
        if (hasTarget && currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (!currentTarget.activeInHierarchy || dist > detectionRange + 15f)
            {
                hasTarget = false;
                currentTarget = null;
            }
            else return;
        }

        // Prioritera Spelaren
        Collider[] players = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        if (players.Length > 0)
        {
            currentTarget = players[0].gameObject;
            hasTarget = true;
            targetIsPlayer = true; // Kom ihåg att det är spelaren
            return;
        }

        // Annars Rangers
        Collider[] rangers = Physics.OverlapSphere(transform.position, detectionRange, rangerLayer);
        if (rangers.Length > 0)
        {
            currentTarget = rangers[0].gameObject;
            hasTarget = true;
            targetIsPlayer = false; // Det är en ranger
        }
    }

    void HandleCombatFlight()
    {
        // Välj värden baserat på målet
        float currentRadius = targetIsPlayer ? playerOrbitRadius : rangerOrbitRadius;
        float currentHeight = targetIsPlayer ? playerFlyHeight : rangerFlyHeight;

        // Snurra långsammare
        orbitTimer += Time.deltaTime * orbitSpeedMultiplier;

        float x = Mathf.Cos(orbitTimer) * currentRadius;
        float z = Mathf.Sin(orbitTimer) * currentRadius;

        Vector3 targetPos = currentTarget.transform.position + new Vector3(x, currentHeight, z);

        // Mjukare förflyttning
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        Vector3 dirToTarget = (currentTarget.transform.position - transform.position).normalized;

        if (dirToTarget != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
            float angleDiff = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);
            float bank = Mathf.Clamp(angleDiff * 3f, -bankingAmount, bankingAmount);

            targetRot *= Quaternion.Euler(0, 0, -bank);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        if (anime) anime.SetBool("IsShooting", true);
    }

    public void Shooting()
    {
        if (Point1 && Point2) { Fire(Point1); Fire(Point2); }
    }

    void Fire(Transform p)
    {
        GameObject b = Instantiate(BulletPrefab, p.position, p.rotation);
        if (b.GetComponent<Rigidbody>())
            b.GetComponent<Rigidbody>().linearVelocity = p.forward * ShootForce;
        Destroy(b, 4f);
    }
}