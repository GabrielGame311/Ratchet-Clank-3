using UnityEngine;

public class TurretScrow : MonoBehaviour
{
    [Header("Inställningar för Skjutning")]
    public float ShootTime = 0.5f;
    public float ShootSpeed = 20f;
    public GameObject PrefabBullet;
    public Transform ShootPoint;

    [Header("Referenser & Animation")]
    public Animator anime;
    public Transform Head;             // Huvudet/Pipan som roterar och tiltar

    [Header("Rotationshastigheter")]
    public float RotateSpeed = 8f;     // Hastighet när den låser och siktar på en fiende

    [Header("R&C 3 Patrullering (Sökning)")]
    [Tooltip("Hur många grader den sveper åt höger och vänster")]
    public float yawSweepAngle = 45f;
    [Tooltip("Hur många grader den tiltar upp och ner")]
    public float pitchSweepAngle = 12f;
    [Tooltip("Hastigheten på svepningen")]
    public float patrolSpeed = 1.8f;
    [Tooltip("Hur många sekunder den pausar i ändlägena för att 'scanna'")]
    public float pauseDuration = 0.5f;

    [Header("Status")]
    public bool isOpen = false;

    private float StartShoot;
    private GameObject Enemie;
    private bool IsenemieTrigger = false;

    // Startvinklar
    private float startYaw;
    private float startPitch;

    // Internt för patrullering och paus
    private float patrolTimer = 0f;
    private float pauseTimer = 0f;
    private bool isPausing = false;
    private float lastSineSign = 0f;

    void Start()
    {
        StartShoot = ShootTime;

        if (Head != null)
        {
            // Spara ursprungsvinklarna så tornet utgår från sin start-rotation
            Vector3 currentEuler = Head.localEulerAngles;
            startYaw = currentEuler.y;
            
            // Konvertera vinkel från 0-360 till -180 till 180 för korrekt X-rotation
            startPitch = (currentEuler.x > 180f) ? currentEuler.x - 360f : currentEuler.x;
        }
    }

    void Update()
    {
        if (isOpen)

        // --- SKJUT- OCH SIKTLÄGE (När fiende finns i zonen) ---
        if (Enemie != null)
        {
            isPausing = false; // Avbryt pauser om en fiende dyker upp

            if (StartShoot <= 0)
            {
                Shoot();
                StartShoot = ShootTime;
            }
            else
            {
                StartShoot -= Time.deltaTime;
            }

            // Sikta direkt mot fienden i 3D (både upp/ner och höger/vänster)
            Vector3 direction = Enemie.transform.position - Head.position;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                Head.rotation = Quaternion.Slerp(Head.rotation, targetRotation, Time.deltaTime * RotateSpeed);
            }
        }
        // --- R&C 3 PATRULLERINGSLÄGE (Sökning) ---
        else
        {
            if (IsenemieTrigger && Enemie == null)
            {
                IsenemieTrigger = false;
                if (anime != null) anime.SetBool("Shoot", false);
            }

            PatrolLogic();
        }
    }

    // Hanterar det klassiska sökmönstret med paus & tilt
    void PatrolLogic()
    {
        if (Head == null) return;

        // 1. Om tornet har nått ett ändläge och ska pausa
        if (isPausing)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPausing = false;
            }
            return; // Står stilla under pausen
        }

        // 2. Öka timern för svepningen
        patrolTimer += Time.deltaTime * patrolSpeed;

        // Sinus- och Cosinus-vågor skapar en mjuk 8-formad/oval rörelse
        float sineVal = Mathf.Sin(patrolTimer);
        float cosVal = Mathf.Cos(patrolTimer * 2f); // Dubbel frekvens gör att den tiltar upp/ner två gånger per svep

        // 3. Detektera när vi når toppen eller botten på sinusvågen (ytterlägena)
        if ((sineVal > 0.97f && lastSineSign <= 0.97f) || (sineVal < -0.97f && lastSineSign >= -0.97f))
        {
            isPausing = true;
            pauseTimer = pauseDuration;
        }
        lastSineSign = sineVal;

        // 4. Räkna ut vinklarna för X (upp/ner) och Y (höger/vänster)
        float yawOffset = sineVal * yawSweepAngle;
        float pitchOffset = cosVal * pitchSweepAngle;

        Head.localRotation = Quaternion.Euler(startPitch + pitchOffset, startYaw + yawOffset, 0f);
    }

    public void OpenScrow()
    {
        isOpen = true;
        if (anime != null) anime.SetBool("Open", true);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemie"))
        {
            IsenemieTrigger = true;
            Enemie = other.gameObject;
            if (anime != null) anime.SetBool("Shoot", true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Enemie)
        {
          //  IsenemieTrigger = false;
            //Enemie = null;
            //if (anime != null) anime.SetBool("Shoot", false);
        }
    }

    public void Shoot()
    {
        if (PrefabBullet != null && ShootPoint != null)
        {
            GameObject bullet = Instantiate(PrefabBullet, ShootPoint.position, ShootPoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = ShootPoint.forward * ShootSpeed;
            }
        }
    }
}