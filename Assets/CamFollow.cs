using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public Transform Player;        // Reference to the player transform
    public Vector3 Offset;          // Offset from the player
    public float FollowSpeed = 5f;  // Speed at which the camera follows

    private Vector3 targetPosition; // Calculated position the camera will move to

    void Start()
    {
        // Set initial camera position relative to the player with the offset
        //transform.position = Player.position + Offset;
    }

    void Update()
    {
        // Calculate the target position based on the player's current position and the offset
        targetPosition = new Vector3(Player.position.x + Offset.x, Player.position.y + Offset.y, Offset.z);

        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, FollowSpeed * Time.deltaTime);
    }
}
