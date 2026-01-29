using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineFreeLook freeLookCamera;
    public Transform player;
    public Transform Target;

    private bool isJumping = false;

    private void Start()
    {

        freeLookCamera.LookAt = Target;
        freeLookCamera.Follow = player;
       
    }

    private void Update()
    {
        // Detect when the player jumps (you should implement this logic)
      

        if (RatchetController.RatchetController_.IsJump)
        {
            // Player has initiated a jump
           
            freeLookCamera.LookAt = null;

            freeLookCamera.Follow = null;
            // Disable the FreeLookCamera while jumping
            //  freeLookCamera.enabled = false;
        }
        else
        {
            freeLookCamera.LookAt = Target;
            freeLookCamera.Follow = player;
            // freeLookCamera.enabled = true;
        }
    }
}
