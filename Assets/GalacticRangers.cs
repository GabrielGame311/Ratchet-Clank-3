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
    public float obstacleCheckDistance = 2f; // Distance to check for obstacles
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

    public bool RangersModeActive = true;
    public bool ISGravity = true;
    public CharacterController Controller;
    public float raycastYOffset;


    public SplineContainer splineContainer;
    private float splineProgress = 0f;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        StartPatrolTime = PatrolIdleTime;
        Controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        objectRadius = transform.localScale.x / 2f;
        findshoot = GetComponentInChildren<FindShoot>();
        HeadStart = HeadControll.transform.position;
        //HeadRot = HeadControll.transform.rotation;
        sound = GetComponent<AudioSource>();
        startShoot = ShootTime;
        StartPatrol = PatrolTime;
       
    }


    private void FixedUpdate()
    {

       
        if(Crouching)
        {
            Iscrouch = true;
        }

        if(!Iscrouch)
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
                // Crouching logic
                crouchTimer += Time.deltaTime;

                HeadAnime.SetBool("Crouch", true);
                FootAnime.SetBool("Crouch", true);
            }
            else if (uncrouchTimer < 5f)
            {
                // Uncrouching logic
                uncrouchTimer += Time.deltaTime;

                HeadAnime.SetBool("Crouch", false);
                FootAnime.SetBool("Crouch", false);
            }
            else
            {
                // Reset crouch and uncrouch timers
                crouchTimer = 0f;
                uncrouchTimer = 0f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if(RangersModeActive)
        {
            RaycastHit hit;

            // Cast a ray downwards to check if AI is near the ground
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundDistance, groundLayer);

            if (isGrounded)
            {
                rb.isKinematic = true;
                _directionY = -1f;
            }
            else
            {
                rb.isKinematic = false;
            }


            HeadControll.transform.eulerAngles = new Vector3(0, HeadControll.transform.eulerAngles.y, 0);

            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);


            if (enemie != null)
            {

                ShootTime -= Time.deltaTime;
                shootPoint.transform.LookAt(enemie.transform);
                // HeadControll.transform.LookAt(enemie.transform);
                Vector3 direction = (enemie.transform.position - HeadControll.transform.position).normalized;

                // Ber�kna m�lrotationen med riktningen mot spelaren
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                // Roterar objektet l�ngsamt mot spelaren med RotateTowards
                HeadControll.transform.rotation = Quaternion.RotateTowards(HeadControll.transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                //



                findshoot.IsPatrol = false;
                HeadAnime.SetBool("ShootPos", true);
                FootAnime.SetBool("ShootPos", true);
                HeadAnime.SetBool("Run", false);
                FootAnime.SetBool("Run", false);
                rb.isKinematic = true;

            }
            else
            {
                HeadControll.transform.rotation = HeadRot;
                findshoot.IsPatrol = false;
                HeadAnime.SetBool("ShootPos", false);
                FootAnime.SetBool("ShootPos", false);

                if (isMoving == false)
                {
                    if (IsMovingShooting)
                    {
                        if (distanceToTarget <= stoppingDistance)
                        {

                            if (!IsObstacleAhead(out alternativeDirection))
                            {
                                HeadAnime.SetTrigger("Sir");
                                FootAnime.SetTrigger("Sir");
                                StartCoroutine(waitHide());
                            }
                        }
                    }



                }
                if (ShootingPatrolPoint)
                {
                    if (isMoving)
                    {
                        distanceToTarget = Vector3.Distance(transform.position, targetPoint[currentPoint].position);

                        if (distanceToTarget <= stoppingDistance)
                        {
                            // === FIX 1: STOPPA ALL RÖRELSE OCH ANIMATION VID WAYPOINT ===
                            HeadAnime.SetBool("Run", false);
                            FootAnime.SetBool("Run", false);
                            rb.isKinematic = true;

                            // Nollställ eventuell rörelse i kontrollern så han inte glider
                            Vector3 stopMove = new Vector3(0, _directionY, 0);
                            Controller.Move(stopMove * Time.deltaTime);

                            // Mjuk rotation till waypointens exakta riktning
                            Quaternion targetRotation = targetPoint[currentPoint].rotation;
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);

                            if (HeadControll != null)
                            {
                                HeadControll.transform.rotation = Quaternion.RotateTowards(HeadControll.transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
                            }

                            // Kontrollera om kroppen har roterat klart
                            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
                            {
                                isMoving = false; // Vänta på nästa patrullering i idle-läge

                                if (!returning)
                                {
                                    currentPoint = 0;
                                    returning = true;
                                }
                                else
                                {
                                    currentPoint = 1;
                                    returning = false;
                                }
                            }
                        }
                        else
                        {
                            // 1. GRAVITATION
                            if (Controller.isGrounded)
                            {
                                _directionY = -1f;
                            }
                            else
                            {
                                _directionY -= 9f * Time.deltaTime;
                            }

                            // =========================================================================
                            // SPLINE-SYSTEM (Springer direkt längs den utritade kurvan)
                            // =========================================================================
                            if (splineContainer != null)
                            {
                                // Öka framsteget på splinen baserat på MoveSpeed och splinens totala längd
                                float totalLength = splineContainer.CalculateLength();
                                splineProgress += (MoveSpeed / totalLength) * Time.deltaTime;
                                splineProgress = Mathf.Clamp01(splineProgress); // Stoppar vid 1f (slutet)

                                // Hämta den exakta positionen på splinen just nu
                                Vector3 targetPositionOnSpline = splineContainer.EvaluatePosition(splineProgress);

                                // Beräkna riktningen från där han står till nästa punkt på splinen
                                Vector3 moveDirection = (targetPositionOnSpline - transform.position);
                                moveDirection.y = 0; // Ignorera höjdskillnader i rörelsen

                                // Spara avståndet innan vi normaliserar
                                float distanceToPoint = moveDirection.magnitude;
                                moveDirection = moveDirection.normalized;

                                // 2. Rotera kroppen mjukt längs splinens kurva
                                if (moveDirection != Vector3.zero)
                                {
                                    Quaternion lookRotation2 = Quaternion.LookRotation(moveDirection);
                                    transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation2, RotateSpeed * Time.deltaTime);
                                }

                                // 3. Flytta karaktären spikrakt längs linjen
                                Vector3 moveVelocity = new Vector3(moveDirection.x * MoveSpeed, _directionY, moveDirection.z * MoveSpeed);
                                Controller.Move(moveVelocity * Time.deltaTime);

                                // Om han är framme vid slutet av splinen, byt till nästa waypoint
                                if (splineProgress >= 1f || (distanceToPoint < 0.5f && splineProgress > 0.9f))
                                {
                                    // Här lägger du din vanliga kod för när han byter waypoint, t.ex:
                                    // currentPoint++;
                                    // splineProgress = 0f; // Nollställ inför nästa spline
                                }
                            }
                            // =========================================================================

                            // 4. Håll huvudet fokuserat på slutpointen
                            Vector3 targetPos = targetPoint[currentPoint].position;
                            Vector3 headDirection = (targetPos - HeadControll.transform.position).normalized;
                            if (headDirection != Vector3.zero && HeadControll != null)
                            {
                                Quaternion lookRotation = Quaternion.LookRotation(headDirection);
                                HeadControll.transform.rotation = Quaternion.RotateTowards(HeadControll.transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                            }

                            // 5. Animationer
                            HeadAnime.SetBool("Run", true);
                            FootAnime.SetBool("Run", true);
                            HeadAnime.SetBool("ShootPos", false);
                            FootAnime.SetBool("ShootPos", false);
                            rb.isKinematic = false;
                            ContinueMove = false;
                            IsShooting = false;
                        }
                    }
                }
            }


            if (targetPoint != null)
            {
                if (isMoving)
                {
                    if (enemie == null)
                    {
                        if (currentPoint < targetPoint.Length)
                        {
                            if (ShootingPatrolPoint == false)
                            {
                                // Sätt animationer
                                HeadAnime.SetBool("Run", true);
                                FootAnime.SetBool("Run", true);
                                HeadAnime.SetBool("ShootPos", false);
                                FootAnime.SetBool("ShootPos", false);
                                rb.isKinematic = false;
                                ContinueMove = false;
                                IsShooting = false;

                                Vector3 target = targetPoint[currentPoint].position;
                                // Standardriktning rakt mot waypointen
                                Vector3 direction = (target - transform.position).normalized;

                                // === FIX 1: UNDVIK HINDER UTAN ATT KLIPPA IGENOM ===
                                // Om ett hinder är i vägen, justera 'direction'-vektorn istället för att flytta transformen direkt
                                if (IsObstacleAhead(out alternativeDirection) && alternativeDirection != Vector3.zero)
                                {
                                    // Vi lägger till alternativ riktning så han svänger runt hindret
                                    direction = (direction + alternativeDirection * 2f).normalized;
                                }

                                // Räkna ut slutgiltig rörelse och flytta ENBART med CharacterController
                                Vector3 move = new Vector3(direction.x * MoveSpeed, _directionY, direction.z * MoveSpeed);
                                Controller.Move(move * Time.deltaTime);

                                // Rotera mjukt mot rörelseriktningen under färden
                                if (new Vector3(direction.x, 0, direction.z) != Vector3.zero)
                                {
                                    Quaternion targetRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
                                }

                                // Kolla om vi har nått fram till waypointen
                                distanceToTarget = Vector3.Distance(transform.position, targetPoint[currentPoint].position);
                                if (distanceToTarget <= stoppingDistance)
                                {
                                    // === FIX 2: ROTERA EXAKT SOM WAYPOINTENS BLÅA PIL ===
                                    // När vi är framme kopierar vi waypointens exakta rotation
                                    transform.rotation = targetPoint[currentPoint].rotation;
                                    if (HeadControll != null)
                                    {
                                        HeadControll.transform.rotation = targetPoint[currentPoint].rotation;
                                    }

                                    if (ShootingPatrolPoint == false)
                                    {
                                        isMoving = false;
                                        currentPoint++;
                                    }

                                    if (Crouching)
                                    {
                                        HeadAnime.SetBool("Crouch", true);
                                    }

                                    if (currentPoint < targetPoint.Length)
                                    {
                                        isMoving = false;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Stoppa animationer om vi står stilla
                    HeadAnime.SetBool("Run", false);
                    FootAnime.SetBool("Run", false);
                    rb.isKinematic = true;

                    if (ShootingPatrolPoint)
                    {
                        PatrolIdleTime -= Time.deltaTime;
                        if (PatrolIdleTime <= 0)
                        {
                            PatrolIdleTime = StartPatrolTime;
                            isMoving = true;
                        }
                    }
                }
            }
        }



        if(Controller.isGrounded)
        {
            
        }
        else
        {
            
           
            if(ISGravity)
            {
                _directionY -= Gravity * Time.deltaTime;
            }

        }















        if (target == null)
        {
            return;
        }

        // Din gamla kod fortsätter här under precis som vanligt:
        Vector3 targetPosition = target.position;
        Vector3 directionToTarget = targetPosition - transform.position;
        float distanceToTarget1 = directionToTarget.magnitude;

        if (distanceToTarget1 > maxDistance)
        {
            // Calculate the desired movement direction
            Vector3 desiredDirection = directionToTarget.normalized;

            // Check if there is a wall between the enemy and the target position
            if (Physics.Linecast(transform.position, targetPosition, wallLayer))
            {
                // If there is a wall, find a new position to move around it
                Vector3 newPath = FindPathAroundWall(targetPosition);
                if (newPath != Vector3.zero)
                {
                    targetPosition = newPath;
                    desiredDirection = (targetPosition - transform.position).normalized;
                }
            }

            // Move the enemy towards the target position
            transform.position += desiredDirection * movementSpeed * Time.deltaTime;
        }
        //Shoot






        //if (!hasBeenAdded && MissionSound.MissionSound_.i == 1)
        // {
        // Execute the code only once
        // ...

        // Set the flag to true to indicate that the integer has been added
        // if (targetPoint.Length > 0)
        // {
        // hasBeenAdded = true;
        // isMoving = true;
        // }
        // }





        foreach (TriggerEnemies trigger in GameObject.FindObjectsOfType<TriggerEnemies>())
        {

            
                if (ContinueMove)
                {
                   // HeadAnime.SetBool("Run", ContinueMove);
                   // FootAnime.SetBool("Run", ContinueMove);
                    isMoving = false;
                    transform.position = Vector3.MoveTowards(transform.position, targetPoint[currentPoint].position, MoveSpeed * Time.deltaTime);
                    transform.LookAt(targetPoint[currentPoint]);

                    float distanceToTarget = Vector3.Distance(transform.position, targetPoint[currentPoint].position);
                    if (distanceToTarget <= stoppingDistance)
                    {
                    // We have reached the target point, stop moving

                        ContinueMove = false;
                    }


                }
                else
                {
                   // HeadAnime.SetBool("Run", ContinueMove);
                   // FootAnime.SetBool("Run", ContinueMove);

                }
            

            
        }

        if (enemie == null)
        {
            IstWalking = true;

            if (IstWalking)
            {
                float distanceToPoint = Vector3.Distance(transform.position, WalkPoint[patrols].position);
                transform.position = Vector3.MoveTowards(transform.position, WalkPoint[patrols].position, WalkSpeed * Time.deltaTime);
                transform.LookAt(WalkPoint[patrols]);
                HeadAnime.SetBool("Walk", true);
                FootAnime.SetBool("Walk", true);


                if (distanceToPoint <= 1)
                {
                    patrols++;
                    if (patrols >= WalkPoint.Length) // Reached the end of the WalkPoint array
                    {
                        patrols = 0; // Reset to point 0
                    }
                }
            }




        }
        else
        {
            HeadAnime.SetBool("Walk", false);
            FootAnime.SetBool("Walk", false);
            IstWalking = false;
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

        // Raycast forward from the character's position
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleCheckDistance))
        {
            // If the ray hits an object tagged as "Wall", return true
            if (hit.collider.CompareTag("Wall"))
            {
                return true; // Obstacle detected
            }
        }

        return false; // No obstacle detected
    }
    private bool IsObstacleAhead(out Vector3 alternativeDirection)
    {
        RaycastHit hit;

        // Cast a ray forward to check for obstacles
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleCheckDistance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                // Obstacle detected, calculate an alternative direction
                alternativeDirection = FindPathAroundWall(hit.point);
                return true; // Obstacle is ahead
            }
        }

        alternativeDirection = Vector3.zero; // No obstacle
        return false;
    }

    private Vector3 FindPathAroundWall(Vector3 obstaclePoint)
    {
        // Calculate an alternative direction to move around the wall
        // In this case, we try to move slightly left or right of the obstacle
        Vector3 rightDirection = transform.right; // Move to the right side of the obstacle
        Vector3 leftDirection = -transform.right; // Move to the left side of the obstacle

        // Cast rays to both the left and right to check for clear paths
        if (!Physics.Raycast(transform.position, rightDirection, obstacleCheckDistance))
        {
            // Right path is clear
            return rightDirection;
        }
        else if (!Physics.Raycast(transform.position, leftDirection, obstacleCheckDistance))
        {
            // Left path is clear
            return leftDirection;
        }

        // If both sides are blocked, return zero to stop movement
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
        if(other.tag == "Sight")
        {
            //isMoving = false;
           
            enemie = other.gameObject;
            IsShooting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Sight")
        {
            //ContinueMove = true;
            //isMoving = false;
            enemie = null;
           IsShooting = false;
            
        }
    }
}
