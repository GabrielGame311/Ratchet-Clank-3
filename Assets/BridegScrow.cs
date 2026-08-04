using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridegScrow : MonoBehaviour
{
    [Header("Bro & Fiender (Referenser)")]
    public GameObject BrideLeft;
    public GameObject BrideRight;
    public float BridgeSpeed = 5f;
    public float ScrowSpeed = 5f;
    public float targetY = 287.23f;
    public GameObject Enemies;
    public GameObject IOS_UI;
    public static BridegScrow BridgeScrow_;
    public bool Scrowing = false;

    [Header("Kamera & Referenser")]
    public GameObject screwCam;
    public Transform screw;              // Skruven som roterar och sänks (om tom används detta transform)
    public Transform scrowPoint;         // Målposition i botten (valfritt)

    [Header("Finjustering för Spelaren")]
    [Tooltip("Avstånd från skruvens mitt till Ratchet")]
    public float standDistance = 1.4f;   
    [Tooltip("Höjdjustering för spelaren i förhållande till skruven")]
    public float playerHeightOffset = 0f;
    [Tooltip("Rotera Ratchet i grader (t.ex. 0, 45, -45) för att rikta honom rätt")]
    public float playerAngleOffset = 0f; 

    [Header("Finjustering för Skiftnyckeln (Wrench)")]
    public Transform wrench;             // Skiftnyckelns Transform
    [Tooltip("Förskjut nyckelns position i handen (X, Y, Z)")]
    public Vector3 wrenchPositionOffset = Vector3.zero;
    [Tooltip("Rotera nyckeln i handen (X, Y, Z grader)")]
    public Vector3 wrenchRotationOffset = Vector3.zero;
    [Tooltip("Ändra storlek/skala på nyckeln")]
    public Vector3 wrenchScale = Vector3.one;

    [Header("Animation & Tider")]
    [Tooltip("Tid i sekunder för Ratchet att ställa sig i position")]
    public float attachDuration = 0.35f; 
    [Tooltip("Tid i sekunder för Ratchet att gå ur skruv-läget")]
    public float detachDuration = 0.30f; 
    public float rotationSpeed = 150f;
    public float totalRotationNeeded = 720f;

    [Header("Status")]
    public bool IsTrigger = false;
    public bool isInteracting = false;
    public bool IsScrow = false;
    public bool isCrowed = false;

    private Transform player;
    private CharacterController playerCC;
    private Animator anime;
    private Vector3 initialScrewPos;
    private float currentRotatedAngle = 0f;
    private bool isTransitioning = false;

    // Sparade ursprungliga transform-värden för nyckeln
    private Vector3 baseWrenchLocalPos;
    private Quaternion baseWrenchLocalRot;
    private Vector3 baseWrenchLocalScale;

    private float count;

    void Start()
    {
        BridgeScrow_ = this;

        if (screw == null) screw = transform;
        initialScrewPos = screw.position;

        if (Enemies != null) Enemies.SetActive(false);

        count = targetY;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCC = player.GetComponent<CharacterController>();
        }

        GameObject ratchetObj = GameObject.FindGameObjectWithTag("Ratchet");
        if (ratchetObj != null)
        {
            anime = ratchetObj.GetComponent<Animator>();
        }

        // Hitta skiftnyckeln om den inte är tilldelad i Inspector
        if (wrench == null)
        {
            WeaponSwitcher ws = FindObjectOfType<WeaponSwitcher>();
            if (ws != null && ws.Wrench_ != null) wrench = ws.Wrench_.transform;
        }

        // Spara grund-transformen för nyckeln
        if (wrench != null)
        {
            baseWrenchLocalPos = wrench.localPosition;
            baseWrenchLocalRot = wrench.localRotation;
            baseWrenchLocalScale = wrench.localScale;
        }
    }

    void Update()
    {
        // UI-logik
        if (!IsTrigger && IOS_UI != null)
        {
            IOS_UI.SetActive(false);
        }

        if (!IsTrigger || IsScrow) return;

        // Starta/avsluta interaktion med E
        if (Input.GetKeyDown(KeyCode.E) && !isTransitioning)
        {
            if (isInteracting) StopScrewInteraction();
            else StartCoroutine(AttachWrenchSequence());
        }

        if (isInteracting && !isTransitioning)
        {
            HandleScrewProcess();
        }
    }

    void LateUpdate()
    {
        // Uppdatera nyckelns finjustering i realtid så du kan ändra i Inspector medan spelet körs
        if (isInteracting && wrench != null)
        {
            ApplyWrenchOffsets();
        }
    }

    // --- 1. STÄLL RATCHET I EXAKT POSITION ---
    IEnumerator AttachWrenchSequence()
    {
        isInteracting = true;
        isTransitioning = true;

        SetWrenchAnimBool("Scrow", true);

        if (screwCam != null) screwCam.gameObject.SetActive(true);
        if (playerCC != null) playerCC.enabled = false;

        WeaponSwitcher ws = FindObjectOfType<WeaponSwitcher>();
        if (ws != null) ws.WrenchEnable();

        if (anime != null)
        {
            anime.SetBool("Scrow", true);
            anime.SetBool("ScrowRun", false);
        }

        // Beräkna riktning från skruven till spelaren
        Vector3 dirFromScrew = player.position - screw.position;
        dirFromScrew.y = 0;
        if (dirFromScrew == Vector3.zero) dirFromScrew = Vector3.forward;
        dirFromScrew.Normalize();

        Vector3 startPlayerPos = player.position;
        Vector3 targetPlayerPos = screw.position + dirFromScrew * standDistance;
        targetPlayerPos.y = screw.position.y + playerHeightOffset;

        Quaternion startPlayerRot = player.rotation;
        Quaternion targetPlayerRot = Quaternion.LookRotation(-dirFromScrew) * Quaternion.Euler(0, playerAngleOffset, 0);

        float elapsed = 0f;

        while (elapsed < attachDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / attachDuration);

            player.position = Vector3.Lerp(startPlayerPos, targetPlayerPos, t);
            player.rotation = Quaternion.Slerp(startPlayerRot, targetPlayerRot, t);

            ApplyWrenchOffsets();

            yield return null;
        }

        player.position = targetPlayerPos;
        player.rotation = targetPlayerRot;

        isTransitioning = false;
    }

    // --- 2. SKRUVNING & SPRÅNG I CIRKEL ---
    void HandleScrewProcess()
    {
        // Kan drivas av Mouse0, knapptryck eller mobil UI (Scrowing)
        if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.E) || Scrowing)
        {
            float stepAngle = rotationSpeed * Time.deltaTime;
            currentRotatedAngle += stepAngle;

            // Spelaren roterar runt skruven
            player.RotateAround(screw.position, Vector3.down, stepAngle);

            // Behåll spelarens vinkel-offset under språnget
            Vector3 radius = player.position - screw.position;
            radius.y = 0;
            if (radius != Vector3.zero)
            {
                player.rotation = Quaternion.LookRotation(-radius.normalized) * Quaternion.Euler(0, playerAngleOffset, 0);
            }

            // Rotera skruven
            screw.Rotate(Vector3.down, stepAngle, Space.World);

            // Beräkna hur långt skruven har kommit (0 till 1)
            float progress = Mathf.Clamp01(currentRotatedAngle / totalRotationNeeded);

            // Sänk skruven/bron
            if (scrowPoint != null)
            {
                screw.position = Vector3.Lerp(initialScrewPos, scrowPoint.position, progress);
            }
            else
            {
                // Sänk utifrån targetY om ingen scrowPoint tilldelats
                Vector3 targetPos = new Vector3(screw.position.x, targetY, screw.position.z);
                screw.position = Vector3.Lerp(initialScrewPos, targetPos, progress);
            }

            // UI Slider uppdatering (om den används)
            count -= 50 * Time.deltaTime;
            if (IOSController.IosController_ != null && IOSController.IosController_.Slider_ != null)
            {
                IOSController.IosController_.Slider_.value = count;
            }

            if (anime != null) anime.SetBool("ScrowRun", true);

            // Om skruvningen är klar
            if (currentRotatedAngle >= totalRotationNeeded || screw.position.y <= targetY)
            {
                CompleteScrewMission();
            }
        }
        else
        {
            if (anime != null) anime.SetBool("ScrowRun", false);
        }
    }

    // Applicera skiftnyckelns offset och skala
    void ApplyWrenchOffsets()
    {
        if (wrench == null) return;

        wrench.localPosition = baseWrenchLocalPos + wrenchPositionOffset;
        wrench.localRotation = baseWrenchLocalRot * Quaternion.Euler(wrenchRotationOffset);
        wrench.localScale = Vector3.Scale(baseWrenchLocalScale, wrenchScale);
    }

    // Återställ nyckeln till sin ursprungliga transform
    void ResetWrenchTransform()
    {
        if (wrench == null) return;

        wrench.localPosition = baseWrenchLocalPos;
        wrench.localRotation = baseWrenchLocalRot;
        wrench.localScale = baseWrenchLocalScale;
    }

    private void SetWrenchAnimBool(string paramName, bool value)
    {
        if (wrench != null)
        {
            Animator wAnim = wrench.GetComponent<Animator>();
            if (wAnim != null) wAnim.SetBool(paramName, value);
        }
    }

    // --- 3. AVSLUTA INTERAKTION ---
    void StopScrewInteraction()
    {
        SetWrenchAnimBool("Scrow", false);
        StartCoroutine(DetachWrenchSequence(false));
    }

    void CompleteScrewMission()
    {
        IsScrow = true;
        isCrowed = true;

        if (scrowPoint != null) screw.position = scrowPoint.position;
        else screw.position = new Vector3(screw.position.x, targetY, screw.position.z);

        SetWrenchAnimBool("Scrow", false);

        // Aktivera uppdragseffekter från BridegScrow
        if (MissionSound.MissionSound_ != null)
        {
            MissionSound.MissionSound_.i++;
            MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
        }

        if (Enemies != null) Enemies.SetActive(true);

        SpawnTime spawnTime = FindObjectOfType<SpawnTime>();
        if (spawnTime != null) spawnTime.RangerTalk();

        StartCoroutine(DetachWrenchSequence(true));
    }

    IEnumerator DetachWrenchSequence(bool isMissionComplete)
    {
        isTransitioning = true;
        isInteracting = false;

        if (anime != null)
        {
            anime.SetBool("Scrow", false);
            anime.SetBool("ScrowRun", false);
        }

        yield return new WaitForSeconds(detachDuration);

        ResetWrenchTransform();

        if (screwCam != null) screwCam.gameObject.SetActive(false);
        if (playerCC != null) playerCC.enabled = true;

        isTransitioning = false;

        if (isMissionComplete)
        {
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IsTrigger = true;
            player = other.transform;
            playerCC = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isInteracting && !isTransitioning)
        {
            IsTrigger = false;
        }
    }
}