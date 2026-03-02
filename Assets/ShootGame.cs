using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class ShootGame : MonoBehaviour
{

    public Transform[] PointSpawn;      // Array of spawn points
    public GameObject RedSpawn;         // Red spawn prefab
    public GameObject GreenSpawn;       // Green spawn prefab

    public float MoveSpeed;             // Movement speed
    public float TimeMove;              // Time for movement

    // Bullet shooting variables
    public AudioSource Sound;
    public AudioClip ShootSoundRed;
    public AudioClip ShootSoundGreen;
    public AudioClip ControllSound;
    public AudioClip ControllSound2;
    public float ShootSpeed;
    public Transform ShootPoint;        // Where the bullet spawns
    public GameObject ShootBulletRed;
    public GameObject ShootBulletGreen;
    private float currentAngle = 0f;
    public GameObject Circle;
    public float RotatePlace;
    public float RotateSpeed;
    // Bullet prefab
    public int RandomSpawn;             // For random spawn selection (not used here anymore)
    public float RotateDuration = 0.25f; // Duration of the rotation in seconds
    private bool isRotating = false; // Prevents overlapping rotations
    public float SpawnTime;
    public float StartTimeSpawn;

    public float TimeSpawn;             // Cooldown time between shots/spawns
    private float nextShootTime;        // Tracks when we can shoot again
    private float nextSpawnTime;        // Tracks when we can spawn again

    float startTime;
    public float TimeShoot;
    float StartShootTime;
    public GameObject RotateObject1;
    public GameObject RotateObject2;
    bool isShoot = true;

    public PlayableDirector PLaying;
    public Camera MainCamera_;
    public Transform[] Circles_;


    public KeyCode ShootRed;
    public KeyCode ShootGreen;

    void Start()
    {
        StartTimeSpawn = SpawnTime;
        StartShootTime = TimeShoot;
        startTime = TimeSpawn;          // Store initial spawn/shoot cooldown
        nextShootTime = 0f;             // Initialize shoot timer
        nextSpawnTime = 0f;             // Initialize spawn timer
    }

    void Update()
    {
        // Handle shoot ing with left mouse button


        if(isShoot)
        {


            if (Input.GetKeyDown(ShootRed))
            {
                Shoot(ShootBulletRed);
                Sound.PlayOneShot(ShootSoundRed);
                isShoot = false;
                nextShootTime = Time.time + TimeSpawn; // Set next allowed shoot time
            }
            if (Input.GetKeyDown(ShootGreen))
            {
                Shoot(ShootBulletGreen);
                Sound.PlayOneShot(ShootSoundGreen);
                isShoot = false;
                nextShootTime = Time.time + TimeSpawn; // Set next allowed shoot time
            }

        }
        else
        {
            TimeShoot -= Time.deltaTime;
        }

        if(TimeShoot < 0)
        {

            isShoot = true;
            TimeShoot = StartShootTime;

        }


        SpawnTime -= Time.deltaTime;

        // Handle random spawning
        if (SpawnTime< 0)
        {
            if(2 < StartTimeSpawn)
            {
                StartTimeSpawn -= 1;
            }
            SpawnTime = StartTimeSpawn;

            SpawnRandomObject();           // Spawn a random object
            
        }
        if (!isRotating)
        {
           
            if (Input.GetKey(KeyCode.A))
            {
                
                StartCoroutine(RotateSmoothly(RotatePlace));
              
                StartCoroutine(RotateSmoothly2(RotatePlace));
            }
            if (Input.GetKey(KeyCode.D))
            {
                
                StartCoroutine(RotateSmoothly(-RotatePlace));
                StartCoroutine(RotateSmoothly2(-RotatePlace));
            }
        }
    }


    private IEnumerator RotateSmoothly(float angleChange)
    {
        isRotating = true; // Lock further rotations

        float lastPlayedAngle = float.MinValue; // Track the last angle where sound was played

        while (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) // Continue while A or D is held
        {
            
                Sound.PlayOneShot(ControllSound);

                
            
            Quaternion startRotation = Circle.transform.rotation; // Starting rotation
            Quaternion endRotation = startRotation * Quaternion.Euler(angleChange, 0, 0); // Target rotation
            float elapsedTime = 0f;

            // Smoothly rotate to the next increment
            while (elapsedTime < RotateDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / RotateDuration; // Interpolation factor (0 to 1)
                Circle.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);


                // Get current absolute rotation angle
                float currentAngle = Quaternion.Angle(Quaternion.identity, Circle.transform.rotation);

                // Calculate the nearest 45-degree multiple
                float nearest45 = Mathf.Round(currentAngle / 45f) * 45f;

                // Check if we've crossed a 45-degree increment with a small threshold
                if (Mathf.Abs(currentAngle - nearest45) < 0.5f && // Small threshold for detection
                    Mathf.Abs(lastPlayedAngle - nearest45) > 1f) // Ensure we don't replay at the same angle
                {
                    
                        Sound.PlayOneShot(ControllSound2);
                    
                    lastPlayedAngle = nearest45; // Update last played angle
                }

                yield return null; // Wait for the next frame
            }

            // Ensure it ends exactly at the target rotation
            Circle.transform.rotation = endRotation;
        }

        isRotating = false; // Unlock when key is released
    }

    private IEnumerator RotateSmoothly2(float angleChange)
    {

        isRotating = true; // Lock further rotations

           
               

                while (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) // Continue while A or D is held
                {
                    Quaternion startRotation = RotateObject1.transform.rotation; // Starting rotation 
                    RotateObject2.transform.rotation = RotateObject1.transform.rotation;
                    Quaternion endRotation = startRotation * Quaternion.Euler(angleChange, 0, 0); // Target rotation
                    float elapsedTime = 0f;

                    // Smoothly rotate to the next increment
                    while (elapsedTime < RotateDuration)
                    {
                        elapsedTime += Time.deltaTime;
                        float t = elapsedTime / RotateDuration; // Interpolation factor (0 to 1)
                        RotateObject1.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
                        RotateObject2.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t);
                        yield return null; // Wait for the next frame
                    }

                    // Ensure it ends exactly at the target rotation
                    RotateObject1.transform.rotation = endRotation;
                }

                
            



        isRotating = false; // Unlock when key is released


    }

    public void Shoot(GameObject bullet)
    {
        // Spawn the bullet at ShootPoint
       GameObject obj = Instantiate(bullet, ShootPoint.position, ShootPoint.rotation);
        obj.GetComponent<Rigidbody>().linearVelocity = ShootPoint.transform.forward * ShootSpeed;
        obj.transform.parent = Circle.transform;
        Destroy(obj, 2);
    }

    public void SpawnRandomObject()
    {
        // Pick a random spawn point from the PointSpawn array
        int randomPointIndex = Random.Range(0, PointSpawn.Length);
        Transform spawnPoint = PointSpawn[randomPointIndex];

        // Randomly choose between RedSpawn and GreenSpawn (50/50 chance)
        GameObject objectToSpawn = (Random.value > 0.5f) ? RedSpawn : GreenSpawn;

        // Instantiate the chosen object at the random spawn point
      GameObject obj = Instantiate(objectToSpawn, spawnPoint.position, spawnPoint.rotation);
        //obj.GetComponent<Rigidbody>().velocity = spawnPoint.transform.forward * MoveSpeed;
        obj.transform.parent = Circle.transform;
        Destroy(obj, 15);

    }
}
