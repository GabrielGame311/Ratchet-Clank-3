using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenAxe : MonoBehaviour
{

    public float TakeDamage;
    public float RotationSpeed;
    public float MoveSpeed;
    public float radius = 5f;
    private float angle = 0f;
    private Vector3 circleCenter;
    public GameObject[] enemie;
    public float distanceBehind = 5f;
    public int totalEnemies = 5;
    public int enemyIndex = 0;
    GameObject Player_;
    private static GameObject currentChasingEnemy = null;
    public float bobbingHeight = 1f;   // Height of the bobbing motion (controls how much the enemy goes up and down)
    public float bobbingSpeed = 2f;

   
    private float time = 0f;

    //Enemie Distance for Se the Player and how much distance to position the Player/Ratchet.
    public float PlayerDistance;
    public float SePlayerDistance;
    public GameObject CrackedAxe;
    public float ExplodeRange;
    public float ExplodeBackword;
    public Transform referenceEnemy;

    private float fixedHeight;

    // Start is called before the first frame update
    void Start()
    {
        Player_ = GameObject.FindGameObjectWithTag("Player");
        circleCenter = transform.position;
        fixedHeight = transform.position.y;

        angle = (2 * Mathf.PI / totalEnemies) * enemyIndex;
    }

    // Update is called once per frame
    void Update()
    {
        float dis = Vector3.Distance(transform.position, Player_.transform.position);

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        // Distance between the enemy and the player
        if (dis < SePlayerDistance && (currentChasingEnemy == null || currentChasingEnemy == gameObject))
        {
            // If the player is closer than the stored distance, this enemy starts chasing
            if (PlayerDistance < dis && (currentChasingEnemy == null || currentChasingEnemy == gameObject))
            {
                currentChasingEnemy = gameObject;

                // Move towards the player while keeping the fixed height
                Vector3 targetPosition = new Vector3(Player_.transform.position.x, fixedHeight, Player_.transform.position.z);
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    MoveSpeed * Time.deltaTime
                );
            }

            // Rotate smoothly to face the player
            Vector3 directionToTarget = Player_.transform.position - transform.position;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    RotationSpeed * Time.deltaTime
                );
            }
        }
        else // If player is out of range
        {
            angle += MoveSpeed * Time.deltaTime / radius;

            // Ensure the angle stays within 0-360 degrees (optional)
            if (angle > 2 * Mathf.PI) angle -= 2 * Mathf.PI;

            // Calculate the new position using circular coordinates (relative to the circle center)
            float x = Mathf.Cos(angle) * radius;  // X position
            float z = Mathf.Sin(angle) * radius;  // Z position

            // Add the bobbing motion to the Y position
            time += Time.deltaTime * bobbingSpeed;  // Update the timer for the bobbing effect
            float yOffset = Mathf.Sin(time) * bobbingHeight;  // Calculate the bobbing height

            // Update the position of the enemy, keeping the Y value oscillating (up and down)
            transform.position = new Vector3(circleCenter.x + x, circleCenter.y + yOffset, circleCenter.z + z);

            // Make the enemy always look in the direction it's moving (tangent to the circle)
            Vector3 direction = new Vector3(-Mathf.Sin(angle), 0, Mathf.Cos(angle)); // Tangent to the circle
            transform.rotation = Quaternion.LookRotation(direction);
        }



    }



    public void TakeDamagePlayer()
    {



    }


    private void OnDestroy()
    {

        gameObject.SetActive(false);
        currentChasingEnemy = null; // Reset the chasing enemy

        // Optionally, notify other enemies to start chasing the player
        NotifyOtherEnemiesToChase();

        GameObject prefab = Instantiate(CrackedAxe, transform.position, transform.rotation);



        Rigidbody rb = prefab.GetComponent<Rigidbody>();

        // Make the enemy fly backward in the direction opposite to its forward vector
        Vector3 backwardForce = -transform.forward * ExplodeBackword; // 'transform.forward' is the direction the enemy is facing
        rb.AddForce(backwardForce, ForceMode.Impulse); // Apply the force instantly (Impulse mode)

        Destroy(prefab, 5);
    }

    void NotifyOtherEnemiesToChase()
    {
        // You can find all other enemies in the scene and assign the chase to one of them
        // For example, find all enemies tagged with "Enemy" and give one of them the task to chase the player
        
        foreach (GameObject enemy in enemie)
        {
            if (enemy != gameObject && Vector3.Distance(enemy.transform.position, Player_.transform.position) < SePlayerDistance)
            {
                currentChasingEnemy = enemy;
                break;
            }
        }
    }

    public void TakeDamageEnemie()
    {

        if(GetComponent<EnemiesHealth>().health < 0)
        {

            // Ensure the Rigidbody component is attached to the CrackedAxe object
           

        }
    }
}
