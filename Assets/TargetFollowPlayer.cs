using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollowPlayer : MonoBehaviour
{
    public Transform target; // The target to follow
    private Vector3 offset; // Offset from the target

    void Start()
    {
        // Calculate the initial offset between the follower and the target
        offset = transform.position - target.position;
    }

    void Update()
    {
        // Get the target's position
        Vector3 targetPosition = target.position;

        // Set the Y position of the follower to its current Y position
        targetPosition.y = transform.position.y;

        // Apply the offset to the target's position
        targetPosition += offset;

        // Update the follower's position
        transform.position = targetPosition;
    }
}
