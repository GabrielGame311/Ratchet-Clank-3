using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFallCam : MonoBehaviour
{

    Transform player;
    public float PosPlayer = 5;
    private Vector3 offset;



    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        offset = transform.position - player.position;
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        float currentDistance = Vector3.Distance(transform.position, player.position);

        // Only follow in the y direction if the distance is greater than the desired distance
        if (currentDistance > PosPlayer)
        {
            // Calculate the new camera position based on the player position and the initial offset
            Vector3 newPosition = new Vector3(player.position.x + offset.x, transform.position.y, player.position.z + offset.z);

            // Set the camera position to the new position
            transform.position = newPosition;
        }
    }
}
