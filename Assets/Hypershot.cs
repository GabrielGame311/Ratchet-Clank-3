using UnityEngine;

public class Hypershot : MonoBehaviour
{
    [Header("Swing Settings")]
    public float swingRange = 20f;       // Räckvidd för att låsa på målet
    public float swingSpeed = 22f;       // Starthastighet framåt
    public float pullForce = 35f;        // Hur starkt linan drar dig mot målet (som i videon)
    public float swingControl = 5f;      // Spelarstyrning i luften
    public float gravity = -12f;         // Lätt tyngdkraft under svingen
    public float damping = 0.4f;         // Luftmotstånd när man hänger kvar

    [Header("References & Layers")]
    public LineRenderer swingLine;
    public Transform gunTip;             // Valfritt: Varifrån lasern skjuts (t.ex. vapnets pip-position)
    public LayerMask targetLayer;        // Layer för Hypershot targets
    public LayerMask groundLayer;

    private CharacterController controller;
    private RatchetController ratchetController;

    private bool isSwinging = false;
    private Transform swingTarget;
    private Vector3 swingAnchor;
    private float swingLength;
    private Vector3 swingVelocity;

    [Header("Beam Animation")]
    public float beamShootSpeed = 120f;  // Hur snabbt laserstrålen skjuter ut från vapnet
    private Vector3 currentBeamEndPos;   // Nuvarande position för strålens spets
    private bool isBeamExtending = false;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = GetComponentInParent<CharacterController>();
        }

        if (controller != null)
        {
            ratchetController = controller.GetComponent<RatchetController>();
        }
    }

    private void Start()
    {
        if (swingLine != null)
        {
            swingLine.enabled = false;
        }
    }

    private void Update()
    {
        // 1. Släpp Mouse0 -> Koppla loss direkt och flyg vidare
        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging)
            {
                StopSwing();
            }
            return;
        }

        // 2. Tryck Mouse0 -> Försök svinga
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            TrySwing();
        }

        // 3. Pågående sving
        if (isSwinging)
        {
            Swing();
            UpdateSwingLine();
        }
    }

    public void TrySwing()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, swingRange, targetLayer);
        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = hit.transform;
            }
        }

        if (closestTarget != null)
        {
            StartSwing(closestTarget);
        }
        else
        {
            ShootRay();
        }
    }

    private void StartSwing(Transform target)
    {
        swingTarget = target;
        swingAnchor = swingTarget.position;
        swingLength = Vector3.Distance(transform.position, swingAnchor);
        isSwinging = true;

        // Starta strålen från vapnets spets
        currentBeamEndPos = (gunTip != null) ? gunTip.position : transform.position;
        isBeamExtending = true;

        if (ratchetController != null)
        {
            ratchetController.CanMove = false;
        }

        Vector3 initialDirection = (transform.forward + Vector3.up * 0.3f).normalized;
        swingVelocity = initialDirection * swingSpeed;
    }

    private void Swing()
    {
        // Vector från spelaren till svingmålet
        Vector3 toAnchor = (swingAnchor - transform.position);
        float currentDistance = toAnchor.magnitude;
        Vector3 pullDirection = toAnchor.normalized;

        // 1. Dragkraft mot målet (skapar den där sugande krafteffekten som i videon)
        swingVelocity += pullDirection * pullForce * Time.deltaTime;

        // 2. Tyngdkraft
        swingVelocity += Vector3.up * gravity * Time.deltaTime;

        // 3. Spelarstyrning
        float inputX = Input.GetAxis("Horizontal") * swingControl;
        float inputZ = Input.GetAxis("Vertical") * swingControl;
        Vector3 controlForce = (transform.right * inputX + transform.forward * inputZ) * Time.deltaTime;
        swingVelocity += controlForce;

        // 4. Dämpning (om man blir hängande under målet stannar farten av)
        swingVelocity *= (1f - damping * Time.deltaTime);

        // Beräkna nästa position
        Vector3 nextPosition = transform.position + swingVelocity * Time.deltaTime;

        // Förhindra att spelaren kommer längre bort än linans ursprungslängd (Rep-fysik)
        Vector3 fromAnchor = nextPosition - swingAnchor;
        if (fromAnchor.magnitude > swingLength)
        {
            nextPosition = swingAnchor + fromAnchor.normalized * swingLength;
            swingVelocity = Vector3.ProjectOnPlane(swingVelocity, fromAnchor.normalized);
        }

        // Flytta CharacterController
        controller.Move(nextPosition - transform.position);

        // Avbryt om spelaren landar på marken
        if (controller.isGrounded && swingVelocity.y <= 0)
        {
            StopSwing();
        }
    }

    public void StopSwing()
    {
        if (!isSwinging) return;

        isSwinging = false;

        if (swingLine != null)
        {
            swingLine.enabled = false;
        }

        // Ge tillbaka spelarkontrollen så man kan gå/hoppa vidare
        if (ratchetController != null)
        {
            ratchetController.CanMove = true;
        }
    }

   private void UpdateSwingLine()
    {
        if (swingLine == null || !isSwinging) return;

        Vector3 startPos = (gunTip != null) ? gunTip.position : transform.position;

        // Om strålen fortfarande skjuter ut sig mot målet:
        if (isBeamExtending)
        {
            // Flytta strålens spets mot målet med hög hastighet
            currentBeamEndPos = Vector3.MoveTowards(currentBeamEndPos, swingAnchor, beamShootSpeed * Time.deltaTime);

            // När strålen har nått fram till targetet, lås den där
            if (Vector3.Distance(currentBeamEndPos, swingAnchor) < 0.1f)
            {
                currentBeamEndPos = swingAnchor;
                isBeamExtending = false;
            }
        }
        else
        {
            // När den väl är framme följer den targetets position exakt
            currentBeamEndPos = swingAnchor;
        }

        // Uppdatera LineRenderer
        swingLine.SetPosition(0, startPos);
        swingLine.SetPosition(1, currentBeamEndPos);
        swingLine.enabled = true;
    }

    private void ShootRay()
    {
        if (SightUI.SightUI_ == null || SightUI.SightUI_.Sight == null) return;

        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 1f);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        Ray ray = new Ray(worldPosition, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            GreenGroundTrigger trigger = hit.collider.GetComponent<GreenGroundTrigger>();
            if (trigger != null)
            {
                trigger.HypershotActive();
            }
        }
    }

    public bool IsSwinging() => isSwinging;
    public Vector3 GetSwingVelocity() => swingVelocity;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, swingRange);
    }
}