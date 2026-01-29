using UnityEngine;
using Cinemachine;

public class CameraJumpControl : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public Transform player;
    private Vector3 cameraOffset;
    private float lockedYPosition;

    void Start()
    {
        // Spara den initiala Y-positionen för kameran
        lockedYPosition = freeLookCamera.transform.position.y;
        cameraOffset = freeLookCamera.transform.position - player.position;
    }

    void Update()
    {
        // Beräkna ny position baserat på spelarens X och Z, men lås Y
        Vector3 targetPosition = new Vector3(player.position.x, lockedYPosition, player.position.z) + cameraOffset;

        // Uppdatera kamerans Follow-position utan att ändra Y
        freeLookCamera.transform.position = targetPosition;

        // Behåll Look At på spelarens X/Z-plan, men inte Y
        Vector3 lookAtPosition = new Vector3(player.position.x, lockedYPosition, player.position.z);
        freeLookCamera.LookAt.position = lookAtPosition;
    }
}