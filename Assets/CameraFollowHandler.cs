using UnityEngine;
using Cinemachine;

public class CameraFollowHandler : MonoBehaviour
{
    public Transform player;        // Dra in spelarens GameObject här
    public float smoothSpeed = 5f;  // Hur snabbt kameran följer Y vid normal rörelse
    private bool isJumping;         // Flagga för hopp
    private float fixedY;           // Den Y-position som låses vid hopp
    private int groundLayer;        // Layer för "Ground"
   
    void Start()
    {
        // Sätt startpositionen till spelarens position
        transform.position = player.position;
        fixedY = transform.position.y;

        // Hämta layer-index för "Ground"
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    void LateUpdate()
    {
        // Kolla om spelaren är på marken eller i luften
        bool isGrounded = IsPlayerGrounded();

        // Om spelaren precis lämnat marken och hoppar
        if (!isGrounded && !isJumping)
        {
            isJumping = true;
            fixedY = transform.position.y; // Lås Y vid hoppets start
        }
        // Om spelaren landar på marken igen
        else if (isGrounded && isJumping)
        {
            isJumping = false;
        }

        // Ny position för CameraTarget
        Vector3 targetPosition;
        if (isJumping)
        {
            // Vid hopp: Följ bara X och Z, behåll låst Y
            targetPosition = new Vector3(player.position.x, fixedY, player.position.z);
        }
        else
        {
            // Vid normal rörelse (på marken eller fall utan hopp): Följ X, Y och Z
            targetPosition = player.position;
        }

        // Smidig övergång till målet
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    // Hjälpmetod för att kolla om spelaren är på "Ground"-layer
    private bool IsPlayerGrounded()
    {
        // Kastar en liten raycast neråt från spelarens position
        RaycastHit hit;
        if (Physics.Raycast(player.position, Vector3.down, out hit, 0.2f))
        {
            // Kolla om träffpunkten är på "Ground"-layer
            return hit.collider.gameObject.layer == groundLayer;
        }
        return false;
    }

    // Metod för att manuellt signalera hopp (valfritt)
    public void SetJumping(bool jumping)
    {
        isJumping = jumping;
        if (jumping) fixedY = transform.position.y;
    }


}
