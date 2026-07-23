using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Movement Settings")]
    public float driveSpeed = 25f;
    public float rotationSpeed = 85f;
    public bool isCar = true;

    [Header("Ground Alignment Settings (Bro / Ramp-lutning)")]
    public LayerMask groundLayer = ~0;    // Välj ett Layer som EJM inkluderar spelaren själv!
    public float rayLength = 3f;          // Hur långt ner strålen kollar
    public float alignSpeed = 8f;         // Hur snabbt bilen lutar sig mot bron/rampen

    [Header("Positions & Camera")]
    public Transform cameraTransform;
    public Transform cameraPos;
    public Transform startPos;

    [Header("Wheel Transforms")]
    public Transform leftWheel;
    public Transform rightWheel;
    public float maxSteerAngle = 25f;
    public float wheelSpinSpeed = 600f;

    [Header("Visual Body Lean (Ratchet & Clank-känsla)")]
    public Transform carBodyModel;
    public float maxTiltAngle = 12f;
    public float tiltSpeed = 8f;

    [Header("Wheel Adjustments")]
    public bool invertSteer = false;
    public bool invertWheelSpin = false;

    private Rigidbody rb;
    private float moveInput;
    private float steerInput;

    private float currentSteerAngle = 0f;
    private float currentWheelSpin = 0f;
    private float currentTilt = 0f;

    private Quaternion initialLeftWheelRot;
    private Quaternion initialRightWheelRot;

    // Sparar den mjukgjorda normalvektorn för att förhindra hack
    private Vector3 smoothedNormal = Vector3.up;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.mass = 1000f;
        rb.linearDamping = 1.2f;
        rb.angularDamping = 2.5f;
        
        // Viktigt för att motverka kamerahack
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (leftWheel != null)  initialLeftWheelRot = leftWheel.localRotation;
        if (rightWheel != null) initialRightWheelRot = rightWheel.localRotation;

        if (startPos != null)
        {
            transform.position = startPos.position;
        }

        smoothedNormal = transform.up;
    }

    void Update()
    {
        if (!isCar) return;

        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");

        AnimateVisuals();
    }

    void FixedUpdate()
    {
        if (!isCar) return;

        HandleMovementAndAlignment();
    }

    void LateUpdate()
    {
        if (!isCar) return;

        UpdateCamera();
    }

    void HandleMovementAndAlignment()
    {
        // 1. Drivning framåt / bakåt
        if (Mathf.Abs(moveInput) > 0.05f)
        {
            Vector3 targetVelocity = transform.forward * moveInput * driveSpeed;
            targetVelocity.y = rb.linearVelocity.y; // Bevara gravitationen
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 10f);
        }

        // 2. Beräkna horisontell svängning (rotera runt bilens nuvarande upp-axel)
        float dir = Vector3.Dot(rb.linearVelocity, transform.forward) < -0.1f ? -1f : 1f;
        float turnAmount = steerInput * rotationSpeed * dir * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.AngleAxis(turnAmount, transform.up);
        Quaternion currentRotation = rb.rotation * turnRotation;

        // 3. Markinriktning med Raycast (RAKT NERÅT i världen för att undvika feedback-loop)
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer, QueryTriggerInteraction.Ignore))
        {
            // Mjukgör ytnormalen så att övergången mellan 3D-polygonerna på bron blir helt hackfri
            smoothedNormal = Vector3.Slerp(smoothedNormal, hit.normal, Time.fixedDeltaTime * alignSpeed);
        }
        else
        {
            // Om bilen flyger i luften, återgå mjukt till rakt upp
            smoothedNormal = Vector3.Slerp(smoothedNormal, Vector3.up, Time.fixedDeltaTime * (alignSpeed * 0.5f));
        }

        // Projicera framåtvektorn på den mjukgjorda marknormalen
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(currentRotation * Vector3.forward, smoothedNormal);

        if (forwardOnPlane.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forwardOnPlane, smoothedNormal);
            rb.MoveRotation(targetRotation);
        }
    }

    void AnimateVisuals()
    {
        // --- A. Hjulsvarvning / Styrning ---
        float targetSteer = steerInput * maxSteerAngle;
        if (invertSteer) targetSteer = -targetSteer;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, Time.deltaTime * 10f);

        // --- B. Hjulrullning ---
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float spinDelta = speed * wheelSpinSpeed * Time.deltaTime;
        if (invertWheelSpin) spinDelta = -spinDelta;
        currentWheelSpin += spinDelta;

        ApplyWheelRotation(leftWheel, initialLeftWheelRot);
        ApplyWheelRotation(rightWheel, initialRightWheelRot);

        // --- C. Karosslutning ---
        if (carBodyModel != null)
        {
            float targetTilt = -steerInput * maxTiltAngle;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
            carBodyModel.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
        }
    }

    void ApplyWheelRotation(Transform wheel, Quaternion initialRotation)
    {
        if (wheel == null) return;

        Quaternion steerRot = Quaternion.Euler(0f, currentSteerAngle, 0f);
        Quaternion spinRot = Quaternion.Euler(currentWheelSpin, 0f, 0f);

        wheel.localRotation = initialRotation * steerRot * spinRot;
    }

    void UpdateCamera()
    {
        if (cameraTransform != null && cameraPos != null)
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, cameraPos.position, Time.deltaTime * 12f);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, cameraPos.rotation, Time.deltaTime * 12f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down * rayLength);
    }
}