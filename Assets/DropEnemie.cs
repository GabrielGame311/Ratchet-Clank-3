using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropEnemie : MonoBehaviour
{
    public Transform dropshipStart;
    public Transform dropshipEnd;
    public float moveSpeed = 10f;
    public float delay = 2f;
    public float initialEnemyDelay = 2f;
    public float additionalEnemyDelay = 2f;

    private float currentDelay;
    private float currentEnemyDelay;
    private int currentEnemyIndex = 0;
    private bool movingToEnd = true;
    public GameObject[] enemies;
    public int Count;
    public Transform SpawnPoint;
    public float rotationSpeed;
    public float DelaySetActive;
    public Animator anime;
    public Transform parent;
    public float jumpForce = 5f;  // Force applied to the enemy when jumping
    public Vector3 jumpDirection = new Vector3(0, 1, 1);
    public float scaleAnimationDuration = 1f;
    public string DropStart;
    public string DropEnd;
    public List<GameObject> SpawnedEnemie;
    private void Start()
    {

     

       

        currentDelay = delay;
        currentEnemyDelay = initialEnemyDelay;
        anime = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
      
            dropshipStart = GameObject.Find(DropStart).transform;
        
          dropshipEnd = GameObject.Find(DropEnd).transform;
        

            if (movingToEnd)
            {
                // Move dropship towards the end point
                transform.position = Vector3.MoveTowards(transform.position, dropshipEnd.position, moveSpeed * Time.deltaTime);
                transform.LookAt(dropshipEnd.transform);
                transform.rotation = dropshipEnd.transform.rotation;


                // If the dropship reaches the end point, activate the enemies and start the initial delay timer
                if (transform.position == dropshipEnd.position)
                {
                    anime.SetBool("Open", true);
                   
                    ActivateEnemies();

                    currentDelay -= Time.deltaTime;

                    if (currentDelay <= 0)
                    {
                        currentEnemyDelay = initialEnemyDelay;
                        currentEnemyIndex = 0;
                        anime.SetBool("Open", false);
                       
                        movingToEnd = false;
                        currentDelay = delay;
                    }
                }
            }
            else
            {





                transform.position = Vector3.MoveTowards(transform.position, dropshipStart.position, moveSpeed * Time.deltaTime);
                transform.LookAt(dropshipStart.transform);


                if (transform.position == dropshipStart.position)
                {
                    Destroy(gameObject);
                }


            }
        


       
    
    
    
    }

    private void OnDestroy()
    {
        GameObject.FindObjectOfType<SpawnTime>().DropshipsSpawned.Remove(gameObject);
    }

    private void ActivateEnemies()
    {
        // Wait for the initial enemy delay before activating the first enemy
        currentEnemyDelay -= Time.deltaTime;

        if (currentEnemyDelay <= 0 && currentEnemyIndex < enemies.Length)
        {

            GameObject enemie = Instantiate(enemies[currentEnemyIndex], SpawnPoint.transform.position, SpawnPoint.transform.rotation);
            enemie.transform.localScale = Vector3.zero;
            enemie.SetActive(true);
            SpawnedEnemie.Add(enemie);
            GetEnemie(enemie);
            //enemies[currentEnemyIndex].SetActive(true);
            StartCoroutine(ScaleEnemy(enemie));

            // Set enemy parent and update index for the next enemy
            enemie.transform.parent = null;
            currentEnemyIndex++;
            currentEnemyDelay = additionalEnemyDelay;
        }
    }

    public void GetEnemie(GameObject enemy)
    {
        GameObject.FindObjectOfType<SpawnTime>().DropshipsSpawned.Add(enemy);
    }
    private IEnumerator ScaleEnemy(GameObject enemy)
    {
        float elapsedTime = 0f;
        Vector3 initialScale = Vector3.zero;
        Vector3 finalScale = Vector3.one;

        // Get the enemy's Rigidbody component to apply force
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb == null)
        {
            // Add a Rigidbody if the enemy doesn't have one
            rb = enemy.AddComponent<Rigidbody>();
        }

        // Apply the jumping force in the specified direction
        rb.AddForce(jumpDirection.normalized * jumpForce, ForceMode.Impulse);

        while (elapsedTime < scaleAnimationDuration)
        {
            // Lerp the scale over time
            enemy.transform.localScale = Vector3.Lerp(initialScale, finalScale, elapsedTime / scaleAnimationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final scale is set exactly to (1, 1, 1)
        enemy.transform.localScale = finalScale;

        // Set parent to null after the scaling is finished
        enemy.transform.parent = null;
    }

}
