using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenGroundTrigger : MonoBehaviour
{
    public float CloseTimeOrOpen = 1f; // Time before the platform goes back up
    private float StartTime;

    public BoxCollider Coll;
    private Animator anime;
    private bool isMoving = false; // To prevent multiple activations

    private GameObject Player_;
    public float DistancePlayer = 20;  // Distance to enable hypershot (optional)
    public float DownPos = 1f;         // How far the platform moves down

    private Vector3 StartPos;

    bool IsOnGround = false;

    void Start()
    {
        StartPos = transform.position; // Save the starting position
        anime = GetComponent<Animator>();
        StartTime = CloseTimeOrOpen;
        Player_ = GameObject.FindGameObjectWithTag("Player");

        // Ensure the collider is NOT a trigger for OnCollisionEnter to work
        if (Coll != null)
            Coll.isTrigger = false;

        // Ensure the animator starts in the correct state
        if (anime != null)
            anime.SetBool("Ground", false); // Start with "Ground" false
    }

    private void Update()
    {
        // Optional hypershot enabling based on distance
        float dis = Vector3.Distance(transform.position, Player_.transform.position);
        if (dis < DistancePlayer)
        {
            GameObject.FindObjectOfType<WeaponSwitcher>()?.HypershotEnable();
        }

        
    }

    IEnumerator Movedown()
    {

        transform.Translate(Vector3.down * 20 * Time.deltaTime);

        // Wait for 2 seconds (as per your original coroutine)
        yield return new WaitForSeconds(2);
        transform.Translate(Vector3.up * 20 * Time.deltaTime);
        // Move back up

    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug to confirm collision detection
        Debug.Log("Collision detected with: " + collision.gameObject.name);

        if (collision.gameObject == Player_)
        {
            StartCoroutine(Movedown());
        }
    }

    IEnumerator Wait()
    {
        Debug.Log("Wait coroutine started");

        // Set animator to "down" state
        if (anime != null)
            anime.SetBool("Ground", true);
        else
            Debug.LogWarning("Animator is null!");

        // Move the platform down
        Vector3 downPosition = new Vector3(StartPos.x, StartPos.y - DownPos, StartPos.z);
        yield return StartCoroutine(MoveObject(transform.position, downPosition, 0.5f)); // 0.5s to move down

        Debug.Log("Platform moved down, waiting for " + CloseTimeOrOpen + " seconds");

        // Wait before moving back up
        yield return new WaitForSeconds(CloseTimeOrOpen);

        // Move the platform back up
        yield return StartCoroutine(MoveObject(transform.position, StartPos, 0.5f)); // 0.5s to move up

        Debug.Log("Platform moved back up");

        // Reset animator state
        if (anime != null)
            anime.SetBool("Ground", false);

        isMoving = false; // Reset moving flag
    }

    IEnumerator MoveObject(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
    }

    public void HypershotActive()
    {
        if (!isMoving)
        {
            Debug.Log("HypershotActive called, starting Wait coroutine");
            isMoving = true;
            StartCoroutine(Wait());
        }
    }
}