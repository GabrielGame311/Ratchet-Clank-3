using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    Vector3 alternativeDirection;
    public bool IsMovingShooting;
    public bool ShootingPatrolPoint;
    public float PatrolIdleTime;
    float StartPatrolTime;

    private bool returning = false;

    // Start is called before the first frame update
    void Start()
    {
        StartPatrolTime = PatrolIdleTime;
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

        RaycastHit hit;

        // Cast a ray downwards to check if AI is near the ground
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundDistance, groundLayer);

        if (isGrounded)
        {
            rb.isKinematic = true;
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
                if(IsMovingShooting)
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

                if(isMoving)
                {
                    distanceToTarget = Vector3.Distance(transform.position, targetPoint[currentPoint].position);

                    if (distanceToTarget <= stoppingDistance)
                    {
                        isMoving = false; // Stanna n�r vi n�r m�let

                        if (!returning)
                        {
                            // Om vi inte �r p� v�g tillbaka, g� till n�sta punkt (punkt 0)
                            currentPoint = 0;
                            returning = true; // Vi har nu g�tt till punkt 0 och �r redo att g� tillbaka
                        }
                        else
                        {
                            // Om vi redan �r p� punkt 0, g� tillbaka till startpunkten (punkt 1)
                            currentPoint = 1;
                            returning = false; // Nu �r vi tillbaka till startpunkten
                        }






                    }
                    else
                    {

                        transform.position = Vector3.MoveTowards(transform.position, targetPoint[currentPoint].position, MoveSpeed * Time.deltaTime);
                        // transform.LookAt(targetPoint[currentPoint]);
                        Vector3 direction2 = (targetPoint[currentPoint].transform.position - transform.position).normalized;

                        // Ber�kna m�lrotationen med riktningen mot spelaren
                        Quaternion lookRotation2 = Quaternion.LookRotation(direction2);

                        // Roterar objektet l�ngsamt mot spelaren med RotateTowards
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation2, RotateSpeed * Time.deltaTime);
                        //HeadControll.transform.LookAt(targetPoint[currentPoint]);
                        Vector3 direction = (targetPoint[currentPoint].transform.position - HeadControll.transform.position).normalized;

                        // Ber�kna m�lrotationen med riktningen mot spelaren
                        Quaternion lookRotation = Quaternion.LookRotation(direction);

                        // Roterar objektet l�ngsamt mot spelaren med RotateTowards
                        HeadControll.transform.rotation = Quaternion.RotateTowards(HeadControll.transform.rotation, lookRotation, RotateSpeed * Time.deltaTime);
                        // Check if we reached the target point



                        // Set running animation
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
                if (currentPoint < targetPoint.Length)
                {
                   
                    // Check for obstacles ahead
                  

                    if(ShootingPatrolPoint == false)
                    {

                        // Set running animation
                        HeadAnime.SetBool("Run", true);
                        FootAnime.SetBool("Run", true);
                        HeadAnime.SetBool("ShootPos", false);
                        FootAnime.SetBool("ShootPos", false);
                        rb.isKinematic = false;
                        ContinueMove = false;
                        IsShooting = false;


                        if (!IsObstacleAhead(out alternativeDirection))
                        {
                            // No obstacle detected, move towards the target point
                            transform.position = Vector3.MoveTowards(transform.position, targetPoint[currentPoint].position, MoveSpeed * Time.deltaTime);
                            transform.LookAt(targetPoint[currentPoint]);
                            HeadControll.transform.LookAt(targetPoint[currentPoint]);

                            // Check if we reached the target point
                            distanceToTarget = Vector3.Distance(transform.position, targetPoint[currentPoint].position);
                            if (distanceToTarget <= stoppingDistance)
                            {
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
                                    isMoving = true; // Move to the next point
                                }
                            }
                        }
                        else if (alternativeDirection != Vector3.zero)
                        {
                            // Obstacle detected, move around the obstacle by adjusting direction
                            transform.position += alternativeDirection * MoveSpeed * Time.deltaTime;
                        }
                        else
                        {
                            // No alternative path available, stop moving
                            HeadAnime.SetBool("Run", false);
                            FootAnime.SetBool("Run", false);
                        }
                    }

                    
                }
            }
            else
            {
                // Stop running animation if not moving
                HeadAnime.SetBool("Run", false);
                FootAnime.SetBool("Run", false);
                rb.isKinematic = true;

                if(ShootingPatrolPoint)
                {

                    PatrolIdleTime -= Time.deltaTime;
                    if (PatrolIdleTime <= 0)
                    {
                        PatrolIdleTime = StartPatrolTime;
                        isMoving = true; // Starta r�relse igen
                       
                    }
                }

            }
        }





        











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
