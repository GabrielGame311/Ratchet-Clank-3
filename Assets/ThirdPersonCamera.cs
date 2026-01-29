using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // Assign the player in the Inspector
    public float distance = 5.0f;
    public float height = 2.0f;
    public float sensitivity = 2.0f;

    private float currentX = 0.0f;
    private float currentY = 20.0f;

    void LateUpdate()
    {
        // Mouse input for rotation
        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, -10f, 60f); // Limit vertical rotation

        // Calculate camera position
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = rotation * Vector3.forward;
        Vector3 position = target.position - direction * distance + Vector3.up * height;

        // Update camera position and rotation
        transform.position = position;
        transform.LookAt(target.position + Vector3.up * 1f); // Look slightly above the player
    }
}