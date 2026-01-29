using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Hypershot : MonoBehaviour
{



    public float swingRange = 10f;      // Radius for OverlapSphere detection
    public float swingSpeed = 10f;      // Speed of swinging motion
    public float swingControl = 2f;     // How much player input affects swing direction
    public float gravity = -9.81f;      // Gravity strength
    public LineRenderer swingLine;      // Optional: Visual line for the grapple
    public LayerMask TargetLayer;
    private CharacterController controller; // Reference to player's CharacterController
    private bool isSwinging = false;
    private Transform swingTarget;
    private Vector3 swingAnchor;        // Point to swing around
    private float swingLength;          // Distance from player to swing point
    private Vector3 swingVelocity;      // Velocity while swinging
    public LayerMask Ground;
    private RatchetController ratchetController;
    private float ropeLength; // Length of the rope during swing

    void Start()
    {
        if (swingLine != null)
        {
            swingLine.enabled = false; // Hide line by default
        }
        ratchetController = controller.GetComponent<RatchetController>();

    }

    void Update()
    {

        if (!isSwinging)
        {
            // Check for swing input (e.g., "E" key)
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                TrySwing();
            }
        }
        else
        {
            Swing();
            UpdateSwingLine();
        }


        if (Input.GetMouseButtonDown(0)) // Correct input check
        {

            ShootRay();



        }

    }

    private void OnEnable()
    {
        controller.GetComponent<RatchetController>().anime.SetBool("Gun", false);
    }

   

    void ShootRay()
    {
        if (SightUI.SightUI_ == null || SightUI.SightUI_.Sight == null)
        {
            Debug.LogError("SightUI är null! Kontrollera att den är korrekt inställd i Unity.");
            return;
        }

        // 1. Testa UI-siktets position
        Vector3 screenPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane + 1f);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        Debug.Log("Screen position: " + screenPosition);
        Debug.Log("World position: " + worldPosition);

        // 2. Skjut en raycast
        Ray ray = new Ray(worldPosition, Camera.main.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f); // Rita rayen i scenen

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, Ground))
        {
            Debug.Log("Träffade marken på: " + hit.point);

            GreenGroundTrigger trigger = hit.collider.GetComponent<GreenGroundTrigger>();
            if (trigger != null)
            {
                trigger.HypershotActive();
            }
            else
            {
                Debug.LogWarning("Ingen GreenGroundTrigger på träffat objekt.");
            }
        }
        else
        {
            Debug.Log("Inget träffat!");
        }
    }

    public void TrySwing()
    {
        // Use OverlapSphere to detect swing points within range
        Collider[] hits = Physics.OverlapSphere(transform.position, swingRange, TargetLayer);
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
            swingTarget = closestTarget;
            swingAnchor = swingTarget.position;
            swingLength = Vector3.Distance(transform.position, swingAnchor);
            isSwinging = true;
            swingVelocity = Vector3.zero; // Reset velocity when starting swing
        }
    }

    void Swing()
    {
        // Calculate direction from player to swing anchor
        Vector3 toAnchor = (swingAnchor - transform.position).normalized;
        float currentDistance = Vector3.Distance(transform.position, swingAnchor);

        // Apply pendulum-like motion
        Vector3 gravityForce = Vector3.down * gravity * Time.deltaTime;
        swingVelocity += gravityForce;

        // Player input to influence swing direction
        float inputX = Input.GetAxis("Horizontal") * swingControl;
        float inputZ = Input.GetAxis("Vertical") * swingControl;
        Vector3 controlForce = (transform.right * inputX + transform.forward * inputZ) * Time.deltaTime;
        swingVelocity += controlForce;

        // Move player with swing velocity
        Vector3 newPosition = transform.position + swingVelocity * Time.deltaTime;

        // Constrain to swing length (like a rope)
        Vector3 fromAnchor = newPosition - swingAnchor;
        if (fromAnchor.magnitude > swingLength)
        {
            newPosition = swingAnchor + fromAnchor.normalized * swingLength;
            swingVelocity = Vector3.ProjectOnPlane(swingVelocity, fromAnchor.normalized);
        }

        // Apply movement
        controller.Move(newPosition - transform.position);

        // Stop swinging if player releases or hits ground
        if (Input.GetKeyUp(KeyCode.E) || controller.isGrounded)
        {
            isSwinging = false;
        }
    }

    void UpdateSwingLine()
    {
        if (swingLine != null && isSwinging)
        {
            swingLine.SetPosition(0, transform.position);
            swingLine.SetPosition(1, swingAnchor);
            swingLine.enabled = true;
        }
        else if (swingLine != null)
        {
            swingLine.enabled = false;
        }
    }

    // Public method to check if swinging (for PlayerController to query)
    public bool IsSwinging()
    {
        return isSwinging;
    }

    // Public method to get swing velocity (for momentum transfer)
    public Vector3 GetSwingVelocity()
    {
        return swingVelocity;
    }

    // Optional: Visualize the OverlapSphere in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, swingRange);
    }
}