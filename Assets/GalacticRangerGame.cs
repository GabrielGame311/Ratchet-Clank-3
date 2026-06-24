using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class GalacticRangerGame : MonoBehaviour
{
    [Header("Waypoint-rutt (Flera punkter)")]
    public List<Transform> Waypoints;
    private int currentWaypointIndex = 0;
    private bool movingForward = true;

    [Header("Instanser & Prefabs")]
    public GameObject Granade;
    public Transform Point;

    public Animator HeadAnime;
    public Animator FootAnime;

    [Header("Rörelse")]
    public float MoveSpeed = 5f;
    public float TurnSpeed = 6f;
    public float ForceBall = 300f;

    [Header("DEDIKERADE LAGER")]
    public LayerMask WallLayer;
    public LayerMask JumpableLayer;   // Denna används även för att känna av marken under honom

    [Header("Sensorer inställningar")]
    public float RayDistance = 4f;
    public float SideRayDistance = 3f;
    public float LowRayHeight = 0.3f;
    public float MidRayHeight = 1.2f;

    [Header("Character Controller Fysik")]
    public float Gravity = -20f;
    public float JumpHeightNormal = 2f;
    public float JumpHeightHigh = 4f;

    [Header("State")]
    public bool StartRun;
    public float WaitForstart;

    private CharacterController controller;
    private GalacticRangers instance;
    private bool isJumping = false;
    private bool isThrowing = false;
    private bool sequenceStarted = false;
    private Vector3 verticalVelocity;

    [Header("NYTT: Idle Loop Inställningar")]
    public float IdleWalkTime = 3f;      // Hur länge han ska gå (i sekunder)
    public float WalkSpeed = 2.5f;       // Hur snabbt han går under promenaden (långsammare än MoveSpeed)
    public float IdleWaitTime = 4f;      // Hur många sekunder han ska stå stilla
    private bool isIdleLooping = false;
    private bool idleShouldWalk = false;
    private Vector3 idleTargetPos;
    private Coroutine idleCoroutine;
    public Transform ThrowToPoint;
    // Räknare för att hålla koll på hur många gånger han sprungit klart
    private int completedRunsCount = 0;

    void Start()
    {
        instance = GetComponent<GalacticRangers>();


        

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (StartRun && !sequenceStarted)
        {
            // Om vi startar en ny rutt, stäng av idle-loopen ifall den är igång
            if (isIdleLooping)
            {
                if (idleCoroutine != null) StopCoroutine(idleCoroutine);
                isIdleLooping = false;
                idleShouldWalk = false;
                HeadAnime.SetBool("Walk", false);
                FootAnime.SetBool("Walk", false);
            }

            if (currentWaypointIndex >= Waypoints.Count - 1)
            {
                movingForward = false;
                currentWaypointIndex = Waypoints.Count - 1;
            }
            else
            {
                movingForward = true;
                currentWaypointIndex = 0;
            }
            if (WaitForstart < 0)
            {
                StartCoroutine(GrenadeAndRunSequence());
            }
        }

        if (!StartRun && sequenceStarted)
        {
            ResetRanger();
        }

        // --- GRAVITATION & AVKÄNNING FÖR ATT HOPPA NERÅT ---
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
            isJumping = false;
        }
        else
        {
            // UTÖKAD: Kolla om han behöver hoppa neråt antingen under löpning ELLER under walk-loopen
            bool canJumpDown = (StartRun && sequenceStarted && !isThrowing) || (isIdleLooping && idleShouldWalk);

            if (!isJumping && canJumpDown && !isIdleLooping)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.8f, JumpableLayer))
                {
                    isJumping = true;
                    HeadAnime.SetTrigger("Jump");
                    FootAnime.SetTrigger("Jump");
                    verticalVelocity.y = Mathf.Sqrt(JumpHeightNormal * 0.4f * -2f * Gravity);
                }
            }

            verticalVelocity.y += Gravity * Time.deltaTime;
        }

        // --- RÖRELSEHANTERING ---
        if (StartRun && sequenceStarted && !isThrowing)
        {
            if (WaitForstart < 0)
            {
                MoveAndAvoidObstacles();
            }
        }
        else if (isIdleLooping && idleShouldWalk)
        {
            // NYTT: Beräkna målorientering och lägg till lasersensorer för promenaden
            Vector3 targetDirection = (idleTargetPos - transform.position).normalized;
            targetDirection.y = 0;
            Vector3 moveDirection = targetDirection;

            // Lasersensorer (Framåt) vid Walk
            Vector3 startOffset = transform.forward * 0.6f;
            Vector3 rayStartLow = transform.position + startOffset + Vector3.up * LowRayHeight;
            Vector3 rayStartMid = transform.position + startOffset + Vector3.up * MidRayHeight;

            Vector3 leftWhiskerDir = (transform.forward - transform.right * 0.6f).normalized;
            Vector3 rightWhiskerDir = (transform.forward + transform.right * 0.6f).normalized;

            RaycastHit wallHit;
            bool hitWallLow = Physics.Raycast(rayStartLow, transform.forward, out wallHit, RayDistance, WallLayer);
            bool hitWallLeft = Physics.Raycast(rayStartMid, leftWhiskerDir, SideRayDistance, WallLayer);
            bool hitWallRight = Physics.Raycast(rayStartMid, rightWhiskerDir, SideRayDistance, WallLayer);

            bool hitJumpLow = Physics.Raycast(rayStartLow, transform.forward, RayDistance, JumpableLayer);
            bool hitJumpMid = Physics.Raycast(rayStartMid, transform.forward, RayDistance, JumpableLayer);

            Debug.DrawRay(rayStartLow, transform.forward * RayDistance, hitWallLow ? Color.red : Color.green);
            Debug.DrawRay(rayStartMid, leftWhiskerDir * SideRayDistance, hitWallLeft ? Color.red : Color.cyan);
            Debug.DrawRay(rayStartMid, rightWhiskerDir * SideRayDistance, hitWallRight ? Color.red : Color.cyan);

            if (hitWallLow)
            {
                Vector3 avoidDir = Vector3.Cross(wallHit.normal, Vector3.up).normalized;
                if (Vector3.Dot(avoidDir, targetDirection) < 0) avoidDir = -avoidDir;
                moveDirection = Vector3.Lerp(moveDirection, avoidDir, 0.8f);
            }
            else if (hitJumpLow && controller.isGrounded && !isJumping)
            {
                if (hitJumpMid) CalculateJumpVelocity(JumpHeightHigh);
                else CalculateJumpVelocity(JumpHeightNormal);
            }

            if (hitWallLeft && !hitWallLow) moveDirection += transform.right * 2f;
            if (hitWallRight && !hitWallLow) moveDirection -= transform.right * 2f;

            moveDirection.Normalize();
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSpeed * Time.deltaTime);
            }

            // Gå framåt med WalkSpeed baserat på den justerade riktningen (transform.forward)
            Vector3 finalMotion = transform.forward * WalkSpeed;
            finalMotion.y = verticalVelocity.y;
            controller.Move(finalMotion * Time.deltaTime);
        }
        else
        {
            // Står stilla (men påverkas av gravitationen)
            controller.Move(verticalVelocity * Time.deltaTime);
        }

        if (StartRun)
        {
            WaitForstart -= Time.deltaTime;
        }
    }

    IEnumerator GrenadeAndRunSequence()
    {
        sequenceStarted = true;

        if (movingForward && Granade != null)
        {
            isThrowing = true;
            if (instance != null) instance.enabled = false;

            HeadAnime.SetTrigger("Throw");
            FootAnime.SetTrigger("Throw");

            yield return new WaitForSeconds(0.4f);
            //ThrowGranade();

            yield return new WaitForSeconds(0.6f);
            isThrowing = false;
        }
        else
        {
            isThrowing = false;
        }
        Destroy(GetComponent<GalacticRangers>());
        HeadAnime.SetBool("Crouch", false);
        FootAnime.SetBool("Crouch", false);
        HeadAnime.SetBool("ShootPos", false);
        FootAnime.SetBool("ShootPos", false);
        HeadAnime.SetBool("Run", true);
        FootAnime.SetBool("Run", true);
    }

    IEnumerator IdleLoopSequence()
    {
        isIdleLooping = true;
        Vector3 startPos = transform.position;

        while (isIdleLooping)
        {
            // 1. Gå framåt i valt antal sekunder
            idleTargetPos = startPos + transform.forward * (WalkSpeed * IdleWalkTime);
            idleShouldWalk = true;
            HeadAnime.SetBool("Walk", true);
            FootAnime.SetBool("Walk", true);

            yield return new WaitForSeconds(IdleWalkTime);

            // 2. Stanna och stå i några sekunder
            idleShouldWalk = false;
            HeadAnime.SetBool("Walk", false);
            FootAnime.SetBool("Walk", false);
            yield return new WaitForSeconds(IdleWaitTime);

            // 3. Gå tillbaka till startpositionen
            idleTargetPos = startPos;
            idleShouldWalk = true;
            HeadAnime.SetBool("Walk", true);
            FootAnime.SetBool("Walk", true);

            yield return new WaitForSeconds(IdleWalkTime);

            // 4. Stanna och stå i några sekunder igen innan loopen börjar om
            idleShouldWalk = false;
            HeadAnime.SetBool("Walk", false);
            FootAnime.SetBool("Walk", false);
            yield return new WaitForSeconds(IdleWaitTime);
        }
    }

    void MoveAndAvoidObstacles()
    {
        if (Waypoints == null || Waypoints.Count == 0) return;

        Transform currentTarget = Waypoints[currentWaypointIndex];
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget < 1.5f)
        {
            if (movingForward)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= Waypoints.Count)
                {
                    currentWaypointIndex = Waypoints.Count - 1;
                    completedRunsCount++;

                    ResetRanger();
                    StartRun = false;

                    if (completedRunsCount > 1)
                    {
                        idleCoroutine = StartCoroutine(IdleLoopSequence());
                    }
                    return;
                }
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 0;
                    completedRunsCount++;

                    ResetRanger();
                    StartRun = false;

                    if (completedRunsCount > 1)
                    {
                        idleCoroutine = StartCoroutine(IdleLoopSequence());
                    }
                    return;
                }
            }
            return;
        }

        Vector3 targetDirection = (currentTarget.position - transform.position).normalized;
        targetDirection.y = 0;
        Vector3 moveDirection = targetDirection;

        // Lasersensorer (Framåt) vid löpning
        Vector3 startOffset = transform.forward * 0.6f;
        Vector3 rayStartLow = transform.position + startOffset + Vector3.up * LowRayHeight;
        Vector3 rayStartMid = transform.position + startOffset + Vector3.up * MidRayHeight;

        Vector3 leftWhiskerDir = (transform.forward - transform.right * 0.6f).normalized;
        Vector3 rightWhiskerDir = (transform.forward + transform.right * 0.6f).normalized;

        RaycastHit wallHit;
        bool hitWallLow = Physics.Raycast(rayStartLow, transform.forward, out wallHit, RayDistance, WallLayer);
        bool hitWallLeft = Physics.Raycast(rayStartMid, leftWhiskerDir, SideRayDistance, WallLayer);
        bool hitWallRight = Physics.Raycast(rayStartMid, rightWhiskerDir, SideRayDistance, WallLayer);

        bool hitJumpLow = Physics.Raycast(rayStartLow, transform.forward, RayDistance, JumpableLayer);
        bool hitJumpMid = Physics.Raycast(rayStartMid, transform.forward, RayDistance, JumpableLayer);

        Debug.DrawRay(rayStartLow, transform.forward * RayDistance, hitWallLow ? Color.red : Color.green);
        Debug.DrawRay(rayStartMid, leftWhiskerDir * SideRayDistance, hitWallLeft ? Color.red : Color.cyan);
        Debug.DrawRay(rayStartMid, rightWhiskerDir * SideRayDistance, hitWallRight ? Color.red : Color.cyan);

        if (hitWallLow)
        {
            Vector3 avoidDir = Vector3.Cross(wallHit.normal, Vector3.up).normalized;
            if (Vector3.Dot(avoidDir, targetDirection) < 0) avoidDir = -avoidDir;
            moveDirection = Vector3.Lerp(moveDirection, avoidDir, 0.8f);
        }
        else if (hitJumpLow && controller.isGrounded && !isJumping)
        {
            if (hitJumpMid) CalculateJumpVelocity(JumpHeightHigh);
            else CalculateJumpVelocity(JumpHeightNormal);
        }

        if (hitWallLeft && !hitWallLow) moveDirection += transform.right * 2f;
        if (hitWallRight && !hitWallLow) moveDirection -= transform.right * 2f;

        moveDirection.Normalize();
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSpeed * Time.deltaTime);
        }

        Vector3 finalMotion = transform.forward * MoveSpeed;
        finalMotion.y = verticalVelocity.y;

        controller.Move(finalMotion * Time.deltaTime);
    }

    void CalculateJumpVelocity(float height)
    {
        isJumping = true;
        HeadAnime.SetTrigger("Jump");
        FootAnime.SetTrigger("Jump");
        verticalVelocity.y = Mathf.Sqrt(height * -2f * Gravity);
    }

    public void ThrowGranade()
    {
        if (Granade != null && Point != null)
        {
            GameObject prefab = Instantiate(Granade, Point.position, Point.rotation);
            Rigidbody rb = prefab.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // 1. STÄNG AV tyngdkraften helt så den aldrig sjunker mot marken
                //rb.useGravity = false;

                // 2. Skjut den spikrakt framåt i extremt hög fart
                rb.linearVelocity = Point.forward * ForceBall;
            }
        }
    }

    void ResetRanger()
    {
        Destroy(GetComponent<GalacticRangers>());
        HeadAnime.SetBool("Crouch", false);
        FootAnime.SetBool("Crouch", false);
        HeadAnime.SetBool("ShootPos", false);
        FootAnime.SetBool("ShootPos", false);
        sequenceStarted = false;
        isThrowing = false;
        isJumping = false;
        verticalVelocity = Vector3.zero;
        if (instance != null) instance.enabled = true;
        HeadAnime.SetBool("Run", false);
        FootAnime.SetBool("Run", false);
        HeadAnime.SetBool("Walk", false);
        FootAnime.SetBool("Walk", false);
    }
}