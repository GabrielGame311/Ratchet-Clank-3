using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // The player transform
    public Vector3 offset; // The offset between the camera and the player

    private float initialCameraY;

    void Start()
    {
        if (player != null)
        {
            // Record the initial Y position of the camera
            initialCameraY = transform.position.y;

            // Calculate initial offset if not provided
            if (offset == Vector3.zero)
            {
                offset = transform.position - player.position;
            }
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // Maintain the initial Y position of the camera, update only X and Z
            Vector3 targetPosition = new Vector3(player.position.x + offset.x, initialCameraY, player.position.z + offset.z);
            transform.position = targetPosition;

            // Optional: if you want the camera to look at the player
            transform.LookAt(player);
        }
        else
        {
            Debug.LogWarning("Player transform is not assigned.");
        }
    }
}
