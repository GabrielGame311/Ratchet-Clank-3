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

    [Header("Combat Settings")]
    [Tooltip("Tid i sekunder mellan varje skott. Högre värde = långsammare skjutande (t.ex. 0.8 eller 1.2).")]
    public float ShootTime = 0.8f;           // Ändrad från 0.32f till 0.8f för rimligare skjuthastighet
    private float shootTimer = 0f;           // Nedräknare för skjutning
    public bool ContinueMove = false;
    public float ShootSpeed = 20f;
    Vector3 HeadStart;
    Quaternion HeadRot;
    FindShoot findshoot;
    GameObject[] Enemies;
    public int TakeDamage = 1;
    public float ShootDis = 20f;

    [Header("Crouching Settings")]
    public bool Crouching = false;
    [Tooltip("Hur länge karaktären står upp och skjuter INNAN den crouching-animerar.")]
    public float StandBeforeCrouchTime = 2.0f; 
    [Tooltip("Hur länge karaktären stannar i crouch-position (utan att skjuta) innan den reser sig igen.")]
    public float CrouchDuration = 3.0f;        
    private float standTimer = 0f;
    private float crouchTimer = 0f;
    private bool isCurrentlyCrouching = false;

    public LayerMask wallLayer;
    public Transform target;
    public float maxDistance = 2f;
    public float movementSpeed = 5f;
    private float objectRadius;
    public float WalkSpeed;
    public float PatrolTime;
    float StartPatrol;
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
    public float splineWaitTimer = 0f;
    public float MAX_SPLINE_WAIT_TIME = 10f;
    public bool IsFreefall = false;
    public bool ContinueWalk = false;

    [Header("Spline Target Settings (Run)")]
    public bool IsRunWithoutShootEnemie = false; 
    public SplineContainer targetSplineContainer;
    private float targetSplineProgress = 0f;
    private bool hasReachedTargetSplineEnd = false;

    private bool hasReachedTargetPointEnd = false;

    private bool splineIntervalPause = false;
    public float splineIntervalPauseTimer = 0f;
    private float nextIntervalDuration = 0f;

    private GameObject lastEnemie;
    private bool isTurningToEnemy = false;
    public float WaitForRun = 0;
    public FindShoot findShootScript;

    [Header("Spline Target Settings (Run List)")]
    public List<SplineContainer> targetSplines; 
    public int currentTargetSplineIndex = 0;   
    private float targetSplineProgress2 = 0f;
    public bool isWaitingForSignal = false;    

    void Start()
    {
        instance = this;
        StartPatrolTime = PatrolIdleTime;
        Controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        objectRadius = transform.localScale.x / 2f;
        findshoot = GetComponentInChildren<FindShoot>();
        if (HeadControll != null) HeadStart = HeadControll.transform.position;
        sound = GetComponent<AudioSource>();
        
        if (ShootTime <= 0.1f) ShootTime = 0.8f;
        shootTimer = ShootTime; // Starta med full fördröjning
        if (ShootDis <= 0.1f) ShootDis = 20f;

        StartPatrol = PatrolTime;
        nextIntervalDuration = Random.Range(5f, 10f);
    }

    private void FixedUpdate()
    {
        // =========================================================
        // CROUCH-LOGIK NÄR FIENDE UPPTÄCKTS
        // =========================================================
        if (enemie != null && Crouching)
        {
            // FAS 1: Stå upp och skjut
            if (standTimer < StandBeforeCrouchTime)
            {
                standTimer += Time.fixedDeltaTime;
                isCurrentlyCrouching = false;

                if (HeadAnime != null) HeadAnime.SetBool("Crouch", false);
                if (FootAnime != null) FootAnime.SetBool("Crouch", false);
            }
            // FAS 2: Ducka och ta skydd (SLUTA SKJUTA)
            else if (crouchTimer < CrouchDuration)
            {
                crouchTimer += Time.fixedDeltaTime;
                
                // Om vi precis gick ner i crouch, sätt flagga och förbered skjut-timern för när vi står upp igen
                if (!isCurrentlyCrouching)
                {
                    isCurrentlyCrouching = true;
                    shootTimer = ShootTime; 
                }

                if (HeadAnime != null) HeadAnime.SetBool("Crouch", true);
                if (FootAnime != null) FootAnime.SetBool("Crouch", true);
            }
            // FAS 3: Res dig upp och börja om
            else
            {
                standTimer = 0f;
                crouchTimer = 0f;
                isCurrentlyCrouching = false;
                shootTimer = ShootTime * 0.5f; // Lite snabbare förstaladdning när han reser sig
            }
        }
        else
        {
            standTimer = 0f;
            crouchTimer = 0f;
            isCurrentlyCrouching = false;

            if (HeadAnime != null) HeadAnime.SetBool("Crouch", false);
            if (FootAnime != null) FootAnime.SetBool("Crouch", false);
        }
    }

    void Update()
    {
        // HÅRD KONTROLL OM MÅLET ÄR GILTIGT OCH INOM RÄCKVIDD
        if (enemie != null)
        {
            float currentDist = Vector3.Distance(transform.position, enemie.transform.position);
            if (!enemie.activeInHierarchy || currentDist > ShootDis)
            {
                enemie = null;
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

            bool isActivelyRunningSingleSpline = (targetSplineContainer != null && !hasReachedTargetSplineEnd && WaitForRun <= 0);
            bool isActivelyRunningSplineList = (targetSplines != null && targetSplines.Count > 0 && currentTargetSplineIndex < targetSplines.Count && !isWaitingForSignal);
            bool isCurrentlyRunning = isActivelyRunningSingleSpline || isActivelyRunningSplineList;

            bool shouldIgnoreEnemies = IsRunWithoutShootEnemie && isCurrentlyRunning;

            // ==========================================
            // STRIDSLOGIK
            // ==========================================
            if (enemie != null && !shouldIgnoreEnemies)
            {
                // SKJUTLOGIK MED KONTROLLERAT INTERVALL
                if (!isCurrentlyCrouching)
                {
                    shootTimer -= Time.deltaTime;
                    if (shootTimer <= 0f)
                    {
                        Shoot();
                        shootTimer = ShootTime; // Återställ timern till hela ShootTime
                    }

                    if (shootPoint != null)
                    {
                        shootPoint.transform.LookAt(enemie.transform);
                    }
                }

                if (findshoot != null) findshoot.IsPatrol = false;

                Vector3 direction = (enemie.transform.position - transform.position);
                direction.y = 0;
                direction = direction.normalized;

                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);

                if (HeadControll != null) HeadControll.transform.localRotation = Quaternion.identity;

                HeadAnime.SetBool("Run", false);
                FootAnime.SetBool("Run", false);
                if (findshoot != null) findshoot.ISRun = false;
                float angleToEnemy = Quaternion.Angle(transform.rotation, lookRotation);

                if (enemie != lastEnemie)
                {
                    isTurningToEnemy = (angleToEnemy > 45f);
                    lastEnemie = enemie;
                }

                if (isTurningToEnemy && enemie == null)
                {
                    HeadAnime.SetBool("ShootPos", false);
                    FootAnime.SetBool("ShootPos", false);
                    HeadAnime.SetBool("Walk", false);
                    FootAnime.SetBool("Walk", true);
                    if (findshoot != null) findshoot.ISRun = false;
                    if (angleToEnemy < 10f) isTurningToEnemy = false;
                }
                else if (isCurrentlyCrouching)
                {
                    HeadAnime.SetBool("ShootPos", false);
                    FootAnime.SetBool("ShootPos", false);
                    HeadAnime.SetBool("Walk", false);
                    FootAnime.SetBool("Walk", false);
                }
                else
                {
                    HeadAnime.SetBool("ShootPos", true);
                    FootAnime.SetBool("ShootPos", true);
                    HeadAnime.SetBool("Walk", false);
                    FootAnime.SetBool("Walk", false);
                    if (findshoot != null) findshoot.ISRun = false;
                }

                rb.isKinematic = true;
            }
            // ==========================================
            // RÖRELSE/PATRULLERING (INGEN FIENDE)
            // ==========================================
            else
            {
                shootTimer = ShootTime * 0.5f; // Sätt en halv fördröjning inför nästa fiendeupptäckt
                lastEnemie = null;
                isTurningToEnemy = false;

                if (HeadControll != null) HeadControll.transform.rotation = HeadRot;
                if (findshoot != null)
                {
                    findshoot.IsPatrol = false;
                    findshoot.ISRun = false;
                }
                HeadAnime.SetBool("ShootPos", false);
                FootAnime.SetBool("ShootPos", false);

                if (targetSplines != null && targetSplines.Count > 0 && currentTargetSplineIndex < targetSplines.Count && isWaitingForSignal)
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
                }
                else if (hasReachedTargetSplineEnd || hasReachedTargetPointEnd)
                {
                    HeadAnime.SetBool("Crouch", false);
                    FootAnime.SetBool("Crouch", false);
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
                else if (targetSplineContainer != null)
                {
                    if (WaitForRun <= 0)
                    {
                        if (findshoot != null) findshoot.ISRun = true;
                        HeadAnime.SetBool("Crouch", false);
                        FootAnime.SetBool("Crouch", false);
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
                    else
                    {
                        WaitForRun -= Time.deltaTime;
                    }
                }
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

                        if (!ContinueWalk)
                        {
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
                        else
                        {
                            if (splineProgress >= 1f) ContinueWalk = true;
                            splineIntervalPauseTimer = 0f;
                        }
                    }
                }
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
                        if (findshoot != null) findshoot.ISRun = true;
                        HeadAnime.SetBool("Run", true);
                        FootAnime.SetBool("Run", true);
                        HeadAnime.SetBool("Crouch", false);
                        FootAnime.SetBool("Crouch", false);
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

        if (targetSplines != null && targetSplines.Count > 0 && currentTargetSplineIndex < targetSplines.Count && targetSplines[currentTargetSplineIndex] != null && !isWaitingForSignal)
        {
            SplineContainer currentSpline = targetSplines[currentTargetSplineIndex];

            HeadAnime.SetBool("Run", true);
            if (findshoot != null) findshoot.ISRun = true;
            FootAnime.SetBool("Run", true);
            HeadAnime.SetBool("Crouch", false);
            FootAnime.SetBool("Crouch", false);
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

            if (targetSplineProgress2 >= 1f || (distToSplineEnd < 0.3f && targetSplineProgress2 > 0.95f))
            {
                targetSplineProgress2 = 0f;
                isWaitingForSignal = true;

                if (EnemiesMission.instance != null)
                {
                    EnemiesMission.instance.isWaitingForNextWave = false;
                }
            }
        }
        else if (targetSplines != null && targetSplines.Count > 0 && currentTargetSplineIndex >= targetSplines.Count)
        {
            isMoving = false;
            HeadAnime.SetBool("Run", false);
            if (findshoot != null) findshoot.ISRun = false;
            FootAnime.SetBool("Run", false);
            HeadAnime.SetBool("Walk", false);
            FootAnime.SetBool("Walk", false);
        }
    }

    public void AdvanceToNextSpline()
    {
        if (isWaitingForSignal && currentTargetSplineIndex < targetSplines.Count)
        {
            isWaitingForSignal = false;
        }
    }

    public void Shoot()
    {
        if (shootPoint == null) return;

        if (sound != null && SoundShootFX != null)
        {
            sound.PlayOneShot(SoundShootFX);
        }

        if (ParticlePrefab != null)
        {
            GameObject particle = Instantiate(ParticlePrefab, shootPoint.transform.position, shootPoint.transform.rotation);
            
            if (particle.TryGetComponent<Rigidbody>(out Rigidbody particleRb))
            {
                particleRb.linearVelocity = shootPoint.transform.forward * ShootSpeed;
            }

            if (particle.TryGetComponent<ParticleDamage>(out ParticleDamage damageScript))
            {
                damageScript.Damage = TakeDamage;
            }

            Destroy(particle, 10f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemie == null && other.CompareTag("Sight"))
        {
            enemie = other.gameObject;
            IsShooting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (enemie != null && other.CompareTag("Sight") && other.gameObject == enemie)
        {
            //enemie = null;
            IsShooting = false;
        }
    }
}