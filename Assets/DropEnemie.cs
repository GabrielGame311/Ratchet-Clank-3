using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothDropship : MonoBehaviour
{
    public enum FinalTurn { Left, Right }

    [Header("Waypoints")]
    public Transform endPoint;
    public Transform startPoint;

    [Header("R&C 3 Exit Settings")]
    public FinalTurn finalTurnDirection = FinalTurn.Right;
    public float finalExitDistance = 150f;
    public float accelerationRate = 15f; // Hur snabbt den ökar farten vid exit
    public float maxExitSpeed = 45f;     // Toppfarten när den flyr

    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float turnSpeed = 3f;
    public float bankingAmount = 35f; // Lite kraftigare lutning för R&C-stil

    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public Transform spawnPoint;
    public float timeBetweenEnemies = 0.8f;

    private enum ShipState { Incoming, Dropping, HeadingToStart, FinalExit, Finished }
    private ShipState currentState = ShipState.Incoming;
    private Animator anim;
    private Vector3 currentTarget;
    private float currentSpeed;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        currentSpeed = moveSpeed;

        if (endPoint != null) currentTarget = endPoint.position;
    }

    void Update()
    {
        if (currentState != ShipState.Dropping && currentState != ShipState.Finished)
        {
            MoveShip();
        }
        else if (currentState == ShipState.Dropping)
        {
            ApplyHoverEffect();
        }
    }

    void MoveShip()
    {
        // Om vi är i FinalExit, öka hastigheten varje frame (Acceleration)
        if (currentState == ShipState.FinalExit)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxExitSpeed, accelerationRate * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, currentSpeed * Time.deltaTime);

        Vector3 direction = (currentTarget - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);

            // BANKING: Räkna ut lutning
            float angleDiff = Vector3.SignedAngle(transform.forward, direction, Vector3.up);

            // I R&C lutar de mer ju snabbare de svänger
            float targetBank = Mathf.Clamp(angleDiff * 2f, -bankingAmount, bankingAmount);
            targetRot *= Quaternion.Euler(0, 0, -targetBank);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }

        if (Vector3.Distance(transform.position, currentTarget) < 2f)
        {
            CheckNextState();
        }
    }

    void CheckNextState()
    {
        switch (currentState)
        {
            case ShipState.Incoming:
                StartCoroutine(DropSequence());
                break;
            case ShipState.HeadingToStart:
                SetupFinalExit();
                break;
            case ShipState.FinalExit:
                currentState = ShipState.Finished;
                Destroy(gameObject);
                break;
        }
    }

    void SetupFinalExit()
    {
        currentState = ShipState.FinalExit;

        // Räkna ut en punkt snett framåt åt sidan för en "arc"-sväng
        Vector3 sideDir = (finalTurnDirection == FinalTurn.Right) ? transform.right : -transform.right;
        Vector3 forwardDir = transform.forward;

        // Kombinera sida och framåt för en snygg kurva utåt
        currentTarget = transform.position + (sideDir + forwardDir).normalized * finalExitDistance;

        // Ge en liten visuell "ryck"-effekt vid start av exit
        currentSpeed += 5f;
    }

    void ApplyHoverEffect()
    {
        // Aggressivt guppande som i R&C
        float hover = Mathf.Sin(Time.time * 2.0f) * 0.4f;
        transform.position = endPoint.position + new Vector3(0, hover, 0);

        // Lite nervöst gungande
        float tilt = Mathf.Sin(Time.time * 1.5f) * 3f;
        transform.rotation = Quaternion.Slerp(transform.rotation, endPoint.rotation * Quaternion.Euler(tilt, 0, tilt), Time.deltaTime);
    }

    IEnumerator DropSequence()
    {
        currentState = ShipState.Dropping;
        if (anim) anim.SetBool("Open", true);
        yield return new WaitForSeconds(1.5f);

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab != null)
            {
                GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
                ApplyRangerJump(enemy);
                yield return new WaitForSeconds(timeBetweenEnemies);
            }
        }

        yield return new WaitForSeconds(0.8f);
        if (anim) anim.SetBool("Open", false);
        yield return new WaitForSeconds(1.0f);

        if (startPoint != null)
        {
            currentTarget = startPoint.position;
            currentState = ShipState.HeadingToStart;
        }
    }

    void ApplyRangerJump(GameObject enemy)
    {
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb)
        {
            // En kraftfull "skjuts" ut ur skeppet
            Vector3 jumpDir = (transform.forward * 8 + Vector3.up * 0.5f).normalized;
            rb.AddForce(jumpDir * 7f, ForceMode.Impulse);
        }
    }
}