using System.Collections;
using UnityEngine;

public class ScrewMission : MonoBehaviour
{
    [Header("Kamera & Referenser")]
    public GameObject screwCam;
    public Transform screw;              // Skruven som roterar och sänks
    public Transform scrowPoint;         // Målpositionen i botten

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

    [Header("Färgändring för material")]
    [Tooltip("Dra in de Renderers du vill ändra färg på")]
    public Renderer[] targetRenderers;
    [Tooltip("Ange material-index för motsvarande Renderer i listan ovan (Rad 0 = Renderer 0, Rad 1 = Renderer 1)")]
    public int[] materialIndexes;
    [Tooltip("Målfärgen när skruven är helt i botten")]
    public Color targetColor = Color.green; 

    [Header("Animation & Tider")]
    [Tooltip("Tid i sekunder för Ratchet att ställa sig i position")]
    public float attachDuration = 0.35f; 
    [Tooltip("Tid i sekunder för Ratchet att gå ur skruv-läget")]
    public float detachDuration = 0.30f; 
    public float rotationSpeed = 150f;
    public float totalRotationNeeded = 720f;

    [Header("Status")]
    public bool isTrigger = false;
    public bool isInteracting = false;
    public bool isScrowed = false;
    public TurretScrow turretScrow;
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

    // Lagra ursprungsfärg & PropertyBlock
    private Color initialColor = Color.white;
    private MaterialPropertyBlock propBlock;

    public bool ISMission;
    public int IndexMission;


    void Start()
    {


        
           
        

        propBlock = new MaterialPropertyBlock();

        if (screw != null) initialScrewPos = screw.position;

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

        // Automatsök Renderers om listan är tom
        if ((targetRenderers == null || targetRenderers.Length == 0) && screw != null)
        {
            targetRenderers = screw.GetComponentsInChildren<Renderer>();
        }

        // Spara ursprunglig färg från första giltiga renderer
        if (targetRenderers != null && targetRenderers.Length > 0)
        {
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer rend = targetRenderers[i];
                int matIndex = (materialIndexes != null && i < materialIndexes.Length) ? materialIndexes[i] : 0;

                if (rend != null && rend.sharedMaterials.Length > matIndex)
                {
                    Material mat = rend.sharedMaterials[matIndex];
                    if (mat != null)
                    {
                        if (mat.HasProperty("_BaseColor"))
                            initialColor = mat.GetColor("_BaseColor");
                        else if (mat.HasProperty("_Color"))
                            initialColor = mat.GetColor("_Color");
                        else
                            initialColor = mat.color;

                        break;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (!isTrigger || isScrowed) return;

        if (Input.GetKeyDown(KeyCode.E) && !isTransitioning)
        {
            if (isInteracting) StopScrewInteraction();
            else StartCoroutine(AttachWrenchSequence());
        }

        if (isInteracting && !isTransitioning) HandleScrewProcess();
    }

    void LateUpdate()
    {
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
        if (Input.GetKey(KeyCode.Mouse0))
        {
            float stepAngle = rotationSpeed * Time.deltaTime;
            currentRotatedAngle += stepAngle;

            player.RotateAround(screw.position, Vector3.down, stepAngle);

            Vector3 radius = player.position - screw.position;
            radius.y = 0;
            if (radius != Vector3.zero)
            {
                player.rotation = Quaternion.LookRotation(-radius.normalized) * Quaternion.Euler(0, playerAngleOffset, 0);
            }

            screw.Rotate(Vector3.down, stepAngle, Space.World);

            float progress = Mathf.Clamp01(currentRotatedAngle / totalRotationNeeded);

            // Uppdatera färgen på alla Renderers baserat på deras index
            UpdateScrewColor(progress);

            if (anime != null) anime.SetBool("ScrowRun", true);

            if (currentRotatedAngle >= totalRotationNeeded)
            {
                CompleteScrewMission();
            }
        }
        else
        {
            if (anime != null) anime.SetBool("ScrowRun", false);
        }
    }

    void UpdateScrewColor(float progress)
    {
        if (targetRenderers == null || targetRenderers.Length == 0) return;

        Color currentColor = Color.Lerp(initialColor, targetColor, progress);

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer rend = targetRenderers[i];
            if (rend == null) continue;

            // Hämtar motsvarande material-index från den andra listan (eller default 0)
            int matIndex = (materialIndexes != null && i < materialIndexes.Length) ? materialIndexes[i] : 0;

            if (rend.sharedMaterials.Length <= matIndex) continue;

            rend.GetPropertyBlock(propBlock, matIndex);

            propBlock.SetColor("_BaseColor", currentColor); // URP / HDRP
            propBlock.SetColor("_Color", currentColor);     // Standard / Built-in

            rend.SetPropertyBlock(propBlock, matIndex);
        }
    }

    void ApplyWrenchOffsets()
    {
        if (wrench == null) return;

        wrench.localPosition = baseWrenchLocalPos + wrenchPositionOffset;
        wrench.localRotation = baseWrenchLocalRot * Quaternion.Euler(wrenchRotationOffset);
        wrench.localScale = Vector3.Scale(baseWrenchLocalScale, wrenchScale);
    }

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
        isScrowed = true;
        if (scrowPoint != null) screw.position = scrowPoint.position;

       turretScrow.OpenScrow();

        UpdateScrewColor(1f);

        SetWrenchAnimBool("Scrow", false);

        SpawnTime spawnTime = FindObjectOfType<SpawnTime>();
        if (spawnTime != null) spawnTime.RangerTalk();

        if (ISMission && MissionSound.MissionSound_ != null)
        {
            MissionSound.MissionSound_.Mission4(IndexMission);

            foreach (ScrewMission scrowMission in FindObjectsOfType<ScrewMission>())
            {
                if (scrowMission != null)
                {
                    scrowMission.ISMission = false;
                }
            }
            ISMission = false;
        }

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
            
            EnemiesMission.instance.ScrewMission_.Remove(GetComponent<ScrewMission>());
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTrigger = true;
            player = other.transform;
            playerCC = other.GetComponent<CharacterController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isInteracting && !isTransitioning)
        {
            isTrigger = false;
        }
    }
}