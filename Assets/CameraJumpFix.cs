using UnityEngine;
using Cinemachine;

public class CameraJumpFix : MonoBehaviour
{
    [SerializeField] private Transform player; // Referens till spelarens transform
    [SerializeField] private float followSpeed = 10f; // Hastighet f�r positionf�ljning
    [SerializeField] private float yFollowSpeed = 6f; // Hastighet f�r vertikal f�ljning
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -5f); // Offset (�ver och bakom)
    [SerializeField] private float groundCheckDistance = 0.5f; // Avst�nd f�r markdetektion
    [SerializeField] private LayerMask groundLayer; // Lager f�r marken
    [SerializeField] private float maxVerticalDistance = 20f; // Max vertikalt avst�nd
    [SerializeField] private float lookAtSpeed = 5f; // Hastighet f�r att rotera och titta p� spelaren

    private Rigidbody playerRb;
    private Vector3 targetPosition;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player reference not set!");
            return;
        }

        playerRb = player.GetComponent<Rigidbody>();
        if (playerRb == null)
        {
            Debug.LogError("Player needs a Rigidbody component!");
            return;
        }

        targetPosition = transform.position;
    }

    void LateUpdate()
    {
        if (player == null || playerRb == null) return;

        // Kolla om spelaren faller
        bool isFalling = playerRb.linearVelocity.y < 0;

        // F�lj spelarens position kontinuerligt, med extra fokus p� fall
        Vector3 playerPosition = player.position;
        Vector3 desiredPosition = new Vector3(
            playerPosition.x + offset.x,
            Mathf.Lerp(transform.position.y, playerPosition.y + offset.y, yFollowSpeed * Time.deltaTime),
            playerPosition.z + offset.z
        );

        // Begr�nsa vertikalt avst�nd om spelaren flyger f�r l�ngt
        float groundY = GetGroundY();
        float verticalDistance = Mathf.Abs(desiredPosition.y - groundY);
        if (verticalDistance > maxVerticalDistance)
        {
            desiredPosition.y = groundY + maxVerticalDistance * Mathf.Sign(playerPosition.y - transform.position.y);
        }

        // Smidig interpolering av position
        targetPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.position = targetPosition;

        // Roterar target f�r att titta p� spelaren, s�rskilt under fall
        if (isFalling)
        {
            Vector3 lookDirection = playerPosition - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
        }

        // Debug f�r att sp�ra
        Debug.Log($"Player Pos: {playerPosition}, Target Pos: {transform.position}, Falling: {isFalling}, Vertical Dist: {verticalDistance}");
    }

    private float GetGroundY()
    {
        if (Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            return hit.point.y;
        }
        return transform.position.y; // Fallback om ingen mark hittas
    }

    void OnDrawGizmos()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(player.position, Vector3.down * groundCheckDistance);
        }
    }
}