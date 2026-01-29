using UnityEngine;
using Cinemachine;

public class MagneticWalk : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravityForce = 10f;
    public LayerMask magneticSurfaceLayer; // Ställ in i Unitys Inspector
    public float magneticAttractionForce = 20f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isOnMagneticSurface = false;
    private Vector3 surfaceNormal;
    private Transform magneticSurface;

    private RatchetController RatchetController_;

    // Cinemachine-kameror
    public CinemachineFreeLook normalCamera;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        RatchetController_ = GetComponent<RatchetController>();
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        moveDirection.Normalize();
        moveDirection = Vector3.ProjectOnPlane(moveDirection, surfaceNormal); // Anpassa efter ytan

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void HandleGravity()
    {
        if (isOnMagneticSurface)
        {
            RatchetController_.enabled = false; // Inaktivera vanlig rörelsekontroll

            velocity = -surfaceNormal * magneticAttractionForce;

            Quaternion targetRotation = Quaternion.LookRotation(Vector3.Cross(transform.right, surfaceNormal), surfaceNormal);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

            // **Byt till magnetkamera**
            normalCamera.m_BindingMode = CinemachineTransposer.BindingMode.LockToTarget;
        }
        else
        {
            velocity += Vector3.down * gravityForce * Time.deltaTime; // Normal gravitation
            RatchetController_.enabled = true;
            normalCamera.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
            // **Byt tillbaka till normal kamera**

        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((1 << hit.gameObject.layer) & magneticSurfaceLayer) != 0)
        {
            isOnMagneticSurface = true;
            surfaceNormal = hit.normal;
            magneticSurface = hit.transform;
        }
        else
        {
            isOnMagneticSurface = false;
            surfaceNormal = Vector3.up;
            magneticSurface = null;
        }
    }
}
