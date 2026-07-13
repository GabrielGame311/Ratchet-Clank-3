using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class GalacticRangers : MonoBehaviour
{
    public float MoveSpeed;
    public GameObject HeadControll;
    public int currentPoint = 0;
    public float RotateSpeed;
    public Animator HeadAnime;
    public Animator FootAnime;
    public Transform[] targetPoint;
    AudioSource sound;
    public AudioClip SoundShootFX;
    public float stoppingDistance;
    public bool isMoving = true;
    public GameObject enemie;
    public Transform shootPoint;
    public GameObject ParticlePrefab;
    public bool IsShooting = false;
    public float ShootTime;
    float startShoot;
    public bool ContinueMove = false;
    public float ShootSpeed;
    Vector3 HeadStart;
    Quaternion HeadRot;
    FindShoot findshoot;
    GameObject[] Enemies;
    public int TakeDamage = 1;
    public float ShootDis;
    public bool Crouching = false;
    float crouchTimer = 0f;
    float uncrouchTimer = 0f;
    bool hasBeenAdded = false;
    public LayerMask wallLayer;
    public Transform target;
    public float maxDistance = 2f;
    public float movementSpeed = 5f;
    private float objectRadius;
    public float WalkSpeed;
    public float PatrolTime;
    float StartPatrol;
    public int patrols;
    bool Iscrouch = false;
    public Transform[] WalkPoint;
    public bool IstWalking = false;
    public float Gravity = -9f;
    public float obstacleCheckDistance = 2f;
    public LayerMask groundLayer;
    private float verticalVelocity;
    public float groundDistance = 0.1f;
    Rigidbody rb;
    private bool isGrounded;
    float distanceToTarget;
    public float _directionY;
    Vector3 alternativeDirection;
    public bool IsMovingShooting;
    public bool ShootingPatrolPoint;
    public float PatrolIdleTime;
    float StartPatrolTime;

    private bool returning = false;
    public static GalacticRangers instance;

    [Header("Rangers Custom Flight/Gravity Settings")]
    public bool RangersModeActive = true;
    public bool ISGravity = true;
    public CharacterController Controller;
    public float raycastYOffset;

    [Header("Spline Patrol Settings (Walk)")]
    public SplineContainer splineContainer;
    private float splineProgress = 0f;
    private bool splineWaitTimerActive = false;
    private float splineWaitTimer = 0f;
    public float MAX_SPLINE_WAIT_TIME = 10f;
    public bool IsFreefall = false;

    [Header("Spline Target Settings (Run)")]
    public SplineContainer targetSplineContainer;
    private float targetSplineProgress = 0f;
    private bool hasReachedTargetSplineEnd = false;

    // Flagga för att låsa karaktären vid målet
    private bool hasReachedTargetPointEnd = false;

    // INTERVALLPAUSER UNDER GÅNGEN
    private bool splineIntervalPause = false;
    private float splineIntervalPauseTimer = 0f;
    private float nextIntervalDuration = 0f;

    // SPÅRA SENASTE FIENDEN OCH ROTATIONSTILLSTÅND
    private GameObject lastEnemie;
    private bool isTurningToEnemy = false;


    [Header("Spline Target Settings (Run List)")]
    public List<SplineContainer> targetSplines; // Lista med våg-splines
    public int currentTargetSplineIndex = 0;   // Vilken spline vi är på
    private float targetSplineProgress2 = 0f;
    public bool isWaitingForSignal = true;     // Startar som true (väntar på att våg 1 rensas)

    void Start()
    {
        instance = this;
        StartPatrolTime = PatrolIdleTime;
        Controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        objectRadius = transform.localScale.x / 2f;
        findshoot = GetComponentInChildren<FindShoot>();
        HeadStart = HeadControll.transform.position;
        sound = GetComponent<AudioSource>();
        startShoot = ShootTime;
        StartPatrol = PatrolTime;

        nextIntervalDuration = Random.Range(5f, 10f);
    }

    private void FixedUpdate()
    {
        if (Crouching)
        {
            Iscrouch = true;
        }

        if (!Iscrouch)
        {
            crouchTimer = 0f;
            uncrouchTimer = 0f;
            HeadAnime.SetBool("Crouch", false);
            FootAnime.SetBool("Crouch", false);
        }
        else
        {
            if (crouchTimer < 5f)
            {
                crouchTimer += Time.deltaTime;
                HeadAnime.SetBool("Crouch", true);
                FootAnime.SetBool("Crouch", true);
            }
            else if (uncrouchTimer < 5f)
            {
                uncrouchTimer += Time.deltaTime;
                HeadAnime.SetBool("Crouch", false);
                FootAnime.SetBool("Crouch", false);
            }
            else
            {
                crouchTimer = 0f;
                uncrouchTimer = 0f;
            }
        }
    }

    void Update()
    {
        // HÅRD KONTROLL OM MÅLET ÄR GILTIGT
        if (enemie != null)
        {
            if (!enemie.activeInHierarchy || Vector3.Distance(transform.position, enemie.transform.position) > 15f)
            {
                //enemie = null;
                IsShooting = false;
                isTurningToEnemy = false;
            }
        }

        if (RangersModeActive)
        {
            RaycastHit hit;
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundDistance, groundLayer);

            if (isGrounded)
            {
                rb.isKinematic = true;
                _directionY = -1f;
            }
            else
            {
                rb.isKinematic = false;
                if (ISGravity)
                {
                    _directionY -= Gravity * Time.deltaTime;
                }
            }

            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            if (HeadControll != null) HeadControll.transform.eulerAngles = new Vector3(0, HeadControll.transform.eulerAngles.y, 0);

            // ==========================================
            // STRIDSLOGIK (OM FIENDE FINNS)
            // ==========================================
            if (enemie != null)
            {
                ShootTime -= Time.deltaTime;
                shootPoint.transform.LookAt(enemie.transform);
                findshoot.IsPatrol = false;

                Vector3 direction = (enemie.transform.position - transform.position);
                direction.y = 0;
                direction = direction.normalized;

                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);

                if (HeadControll != null) HeadControll.transform.localRotation = Quaternion.identity;

                HeadAnime.SetBool("Run", false);
                FootAnime.SetBool("Run", false);

                float angleToEnemy = Quaternion.Angle(transform.rotation, lookRotation);

                if (enemie != lastEnemie)
                {
                    if (angleToEnemy > 45f)
                    {
                        isTurningToEnemy = true;
                    }
                    else
                    {
                        isTurningToEnemy = false;
                    }
                    lastEnemie = enemie;
                }

                if (isTurningToEnemy)
                {
                    HeadAnime.SetBool("ShootPos", false);
                    FootAnime.SetBool("ShootPos", false);
                    HeadAnime.SetBool("Walk", false);
                    FootAnime.SetBool("Walk", true);

                    if (angleToEnemy < 10f)
                    {
                        isTurningToEnemy = false;
                    }
                }
                else
                {
                    HeadAnime.SetBool("ShootPos", true);
                    FootAnime.SetBool("ShootPos", true);
                    HeadAnime.SetBool("Walk", false);
                    HeadAnime.SetBool("Walk", false);
                    FootAnime.SetBool("Walk", false);
                }

                rb.isKinematic = true;
            }
            // ==========================================
            // LOGIK NÄR INGEN FIENDE UPPTÄCKS
            // ==========================================
            else
            {
                lastEnemie = null;
                isTurningToEnemy = false;

                if (HeadControll != null) HeadControll.transform.rotation = HeadRot;
                findshoot.IsPatrol = false;
                HeadAnime.SetBool("ShootPos", false);
                FootAnime.SetBool("ShootPos", false);

                // KONTROLL: VÄNTAR PÅ SIGNAL FRÅN ENEMIES MISSION (VÅGLISTA)?
                if (targetSplines != null && currentTargetSplineIndex < targetSplines.Count && isWaitingForSignal)
                {
                    // Stoppa dem stenhårt på platsen medan de väntar på att vågen ska dö
                    HeadAnime.SetBool("Run", false);
                    FootAnime.SetBool("Run", false);
                    HeadAnime.SetBool("Walk", false);
                    FootAnime.SetBool("Walk", false);
                    isMoving = false;

                    if (Controller != null && Controller.enabled)
                    {
                        Controller.Move(new Vector3(0, _directionY, 0) * Time.deltaTime);
                    }
                }
                // --- 1. GLOBALT TVINGANDE STOPP (När slutet nås på Spline eller TargetPoint) ---
                else if (hasReachedTargetSplineEnd || hasReachedTargetPointEnd)
                {
                    HeadAnime.SetBool("Run", false);
                    FootAnime.SetBool("Run", false);
                    HeadAnime.SetBool("Walk", false);
                    FootAnime.SetBool("Walk", false);
                    isMoving = false;

                    if (Controller != null && Controller.enabled)
                    {
                        Controller.Move(new Vector3(0, _directionY, 0) * Time.deltaTime);
                    }

                    if (hasReachedTargetPointEnd && targetPoint != null && targetPoint.Length > 0 && targetPoint[currentPoint] != null)
                    {
                        Vector3 targetForward = targetPoint[currentPoint].forward;
                        targetForward.y = 0;
                        if (targetForward != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(targetForward);
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
                        }
                    }
                    else if (hasReachedTargetSplineEnd && targetSplineContainer != null)
                    {
                        Vector3 p1 = targetSplineContainer.EvaluatePosition(0.98f);
                        Vector3 p2 = targetSplineContainer.EvaluatePosition(1f);
                        Vector3 splineForward = (p2 - p1).normalized;
                        splineForward.y = 0;
                        if (splineForward != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(splineForward);
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
                        }
                    }
                }
                // --- 2. EXTRA SINGLE TARGET/RUN SPLINE ---
                else if (targetSplineContainer != null)
                {
                    HeadAnime.SetBool("Run", true);
                    FootAnime.SetBool("Run", true);
                    HeadAnime.SetBool("Walk", false);
                    FootAnime.SetBool("Walk", false);
                    rb.isKinematic = false;

                    float totalTargetLength = targetSplineContainer.CalculateLength();
                    targetSplineProgress += (MoveSpeed / totalTargetLength) * Time.deltaTime;
                    targetSplineProgress = Mathf.Clamp01(targetSplineProgress);

                    Vector3 splineTargetPos = targetSplineContainer.EvaluatePosition(targetSplineProgress);
                    Vector3 targetMoveDir = (splineTargetPos - transform.position);
                    targetMoveDir.y = 0;

                    float distToSplineEnd = targetMoveDir.magnitude;
                    targetMoveDir = targetMoveDir.normalized;

                    if (targetMoveDir != Vector3.zero)
                    {
                        Quaternion runRot = Quaternion.LookRotation(targetMoveDir);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, runRot, RotateSpeed * Time.deltaTime);
                    }

                    Vector3 moveVel = new Vector3(targetMoveDir.x * MoveSpeed, _directionY, targetMoveDir.z * MoveSpeed);
                    if (Controller != null && Controller.enabled)
                    {
                        Controller.Move(moveVel * Time.deltaTime);
                    }

                    if (targetSplineProgress >= 1f || (distToSplineEnd < 0.3f && targetSplineProgress > 0.95f))
                    {
                        hasReachedTargetSplineEnd = true;
                    }
                }
                // --- 3. VANLIG PATRULLSPLINE (WALK) ---
                else if (splineContainer != null && !IsFreefall)
                {
                    if (splineWaitTimerActive || splineIntervalPause)
                    {
                        HeadAnime.SetBool("Walk", false);
                        FootAnime.SetBool("Walk", false);
                        HeadAnime.SetBool("Run", false);
                        FootAnime.SetBool("Run", false);

                        splineIntervalPauseTimer += Time.deltaTime;

                        if (splineWaitTimerActive)
                        {
                            if (splineIntervalPauseTimer >= MAX_SPLINE_WAIT_TIME)
                            {
                                splineIntervalPauseTimer = 0f;
                                splineProgress = 0f;
                                splineWaitTimerActive = false;
                                nextIntervalDuration = Random.Range(5f, 10f);
                            }
                        }
                        else if (splineIntervalPause)
                        {
                            if (splineIntervalPauseTimer >= nextIntervalDuration)
                            {
                                splineIntervalPauseTimer = 0f;
                                splineIntervalPause = false;
                                nextIntervalDuration = Random.Range(5f, 10f);
                            }
                        }
                    }
                    else
                    {
                        HeadAnime.SetBool("Walk", true);
                        FootAnime.SetBool("Walk", true);
                        HeadAnime.SetBool("Run", false);
                        FootAnime.SetBool("Run", false);
                        rb.isKinematic = false;

                        splineIntervalPauseTimer += Time.deltaTime;

                        float totalLength = splineContainer.CalculateLength();
                        splineProgress += (WalkSpeed / totalLength) * Time.deltaTime;
                        splineProgress = Mathf.Clamp01(splineProgress);

                        Vector3 targetPositionOnSpline = splineContainer.EvaluatePosition(splineProgress);
                        Vector3 moveDirection = (targetPositionOnSpline - transform.position);
                        moveDirection.y = 0;

                        float distanceToPoint = moveDirection.magnitude;
                        moveDirection = moveDirection.normalized;

                        if (moveDirection != Vector3.zero)
                        {
                            Quaternion targetRot = Quaternion.LookRotation(moveDirection);
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, RotateSpeed * Time.deltaTime);
                        }

                        Vector3 moveVelocity = new Vector3(moveDirection.x * WalkSpeed, _directionY, moveDirection.z * WalkSpeed);
                        if (Controller != null && Controller.enabled)
                        {
                            Controller.Move(moveVelocity * Time.deltaTime);
                        }

                        if (splineProgress >= 1f || (distanceToPoint < 0.3f && splineProgress > 0.95f))
                        {
                            splineIntervalPauseTimer = 0f;
                            splineWaitTimerActive = true;
                        }
                        else if (splineIntervalPauseTimer >= nextIntervalDuration)
                        {
                            splineIntervalPauseTimer = 0f;
                            splineIntervalPause = true;
                            nextIntervalDuration = Random.Range(5f, 10f);
                        }
                    }
                }
                // --- 4. TARGET POINTS ARRAY ---
                else if (targetPoint != null && targetPoint.Length > 0 && isMoving && isGrounded)
                {
                    distanceToTarget = Vector3.Distance(transform.position, targetPoint[currentPoint].position);

                    if (distanceToTarget <= stoppingDistance)
                    {
                        hasReachedTargetPointEnd = true;
                        HeadAnime.SetBool("Run", false);
                        FootAnime.SetBool("Run", false);
                        transform.rotation = targetPoint[currentPoint].rotation;

                        if (Controller != null && Controller.enabled)
                            Controller.Move(new Vector3(0, _directionY, 0) * Time.deltaTime);
                    }
                    else
                    {
                        HeadAnime.SetBool("Run", true);
                        FootAnime.SetBool("Run", true);

                        Vector3 targetPos = targetPoint[currentPoint].position;
                        Vector3 direction = (targetPos - transform.position).normalized;

                        Vector3 move = new Vector3(direction.x * MoveSpeed, _directionY, direction.z * MoveSpeed);
                        if (Controller != null && Controller.enabled) Controller.Move(move * Time.deltaTime);

                        Quaternion targetRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
                    }
                }
                else if (!isGrounded)
                {
                    Vector3 gravityMove = new Vector3(0, _directionY, 0);
                    if (Controller != null && Controller.enabled)
                    {
                        Controller.Move(gravityMove * Time.deltaTime);
                    }
                }
            }
        }

        // =================================================================
        // --- KORRIGERAD: TARGET SPLINES LISTA (RÖRELSE & SIGNAL-KONTROLL) ---
        // =================================================================
        if (targetSplines[currentTargetSplineIndex] != null && currentTargetSplineIndex < targetSplines.Count && !isWaitingForSignal && enemie == null)
        {
            SplineContainer currentSpline = targetSplines[currentTargetSplineIndex];

            HeadAnime.SetBool("Run", true);
            FootAnime.SetBool("Run", true);
            rb.isKinematic = false;

            float totalTargetLength = currentSpline.CalculateLength();
            targetSplineProgress2 += (MoveSpeed / totalTargetLength) * Time.deltaTime;
            targetSplineProgress2 = Mathf.Clamp01(targetSplineProgress2);

            Vector3 splineTargetPos = currentSpline.EvaluatePosition(targetSplineProgress2);
            Vector3 targetMoveDir = (splineTargetPos - transform.position);
            targetMoveDir.y = 0;

            float distToSplineEnd = targetMoveDir.magnitude;
            targetMoveDir = targetMoveDir.normalized;

            if (targetMoveDir != Vector3.zero)
            {
                Quaternion runRot = Quaternion.LookRotation(targetMoveDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, runRot, RotateSpeed * Time.deltaTime);
            }

            Vector3 moveVel = new Vector3(targetMoveDir.x * MoveSpeed, _directionY, targetMoveDir.z * MoveSpeed);
            if (Controller != null && Controller.enabled) Controller.Move(moveVel * Time.deltaTime);

            // Byt spline när roboten har sprungit hela vägen till slutet
            if (targetSplineProgress2 >= 1f || (distToSplineEnd < 0.3f && targetSplineProgress2 > 0.95f))
            {
                currentTargetSplineIndex++; // Flytta fram indexet för nästa spline i kön
                targetSplineProgress2 = 0f;
                isWaitingForSignal = true;  // Sätt till true DIREKT! Nu stannar de och väntar på nästa våg av fiender

                // Säg även till EnemiesMission att vi har stannat och är redo att ta emot en ny signal
                if (EnemiesMission.instance != null)
                {
                    EnemiesMission.instance.isWaitingForNextWave = false;
                }
            }
        }
        // STOPP-LOGIK (Körs endast när alla splines i listan har körts färdigt)
        else if (targetSplines != null && currentTargetSplineIndex >= targetSplines.Count)
        {
            isMoving = false;
            HeadAnime.SetBool("Run", false);
            FootAnime.SetBool("Run", false);
            HeadAnime.SetBool("Walk", false);
            FootAnime.SetBool("Walk", false);
        }
    }

    public void AdvanceToNextSpline()
    {
        if (isWaitingForSignal)
        {
            if (currentTargetSplineIndex < targetSplines.Count)
            {
                isWaitingForSignal = false; // Bryt vänteläget och börja springa nästa spline!
            }
        }
    }

    IEnumerator waitHide()
    {
        yield return new WaitForSeconds(15);
        gameObject.SetActive(false);
    }

    private bool IsObstacleAhead()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleCheckDistance))
        {
            if (hit.collider.CompareTag("Wall")) return true;
        }
        return false;
    }

    private bool IsObstacleAhead(out Vector3 alternativeDirection)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleCheckDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                alternativeDirection = FindPathAroundWall(hit.point);
                return true;
            }
        }
        alternativeDirection = Vector3.zero;
        return false;
    }

    private Vector3 FindPathAroundWall(Vector3 obstaclePoint)
    {
        Vector3 rightDirection = transform.right;
        Vector3 leftDirection = -transform.right;

        if (!Physics.Raycast(transform.position, rightDirection, obstacleCheckDistance)) return rightDirection;
        else if (!Physics.Raycast(transform.position, leftDirection, obstacleCheckDistance)) return leftDirection;

        return Vector3.zero;
    }

    public void Shoot()
    {
        sound.PlayOneShot(SoundShootFX);
        GameObject particle = Instantiate(ParticlePrefab, shootPoint.transform.position, shootPoint.transform.rotation);
        particle.GetComponent<Rigidbody>().linearVelocity = shootPoint.transform.forward * ShootSpeed;
        particle.GetComponent<ParticleDamage>().Damage = TakeDamage;
        Destroy(particle, 10);
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemie == null && other.tag == "Sight")
        {
            enemie = other.gameObject;
            IsShooting = true;
        }
 
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemie != null && other.tag == "Sight")
        {
            enemie = null;
            IsShooting = false;
        }
    }
}