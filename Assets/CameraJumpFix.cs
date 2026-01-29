using UnityEngine;
using Cinemachine;

public class CameraJumpFix : MonoBehaviour
{
    [SerializeField] private Transform player; // Referens till spelarens transform
    [SerializeField] private float followSpeed = 10f; // Hastighet för positionföljning
    [SerializeField] private float yFollowSpeed = 6f; // Hastighet för vertikal följning
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -5f); // Offset (över och bakom)
    [SerializeField] private float groundCheckDistance = 0.5f; // Avstånd för markdetektion
    [SerializeField] private LayerMask groundLayer; // Lager för marken
    [SerializeField] private float maxVerticalDistance = 20f; // Max vertikalt avstånd
    [SerializeField] private float lookAtSpeed = 5f; // Hastighet för att rotera och titta på spelaren

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
        bool isFalling = playerRb.velocity.y < 0;

        // Följ spelarens position kontinuerligt, med extra fokus på fall
        Vector3 playerPosition = player.position;
        Vector3 desiredPosition = new Vector3(
            playerPosition.x + offset.x,
            Mathf.Lerp(transform.position.y, playerPosition.y + offset.y, yFollowSpeed * Time.deltaTime),
            playerPosition.z + offset.z
        );

        // Begränsa vertikalt avstånd om spelaren flyger för långt
        float groundY = GetGroundY();
        float verticalDistance = Mathf.Abs(desiredPosition.y - groundY);
        if (verticalDistance > maxVerticalDistance)
        {
            desiredPosition.y = groundY + maxVerticalDistance * Mathf.Sign(playerPosition.y - transform.position.y);
        }

        // Smidig interpolering av position
        targetPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.position = targetPosition;

        // Roterar target för att titta på spelaren, särskilt under fall
        if (isFalling)
        {
            Vector3 lookDirection = playerPosition - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
        }

        // Debug för att spåra
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