using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeforiusBoss : MonoBehaviour
{

    public Animator[] anime;
    public Transform Target;
    public Transform[] objects;

    GameObject player;
    public float ShootSpeed;
    public GameObject BulletPrefab;
    
    public Transform Point;
    public Transform Point2;

    public float timeBetweenShots = 2f;
    public int numberOfShots = 5;
    public float waitTime = 5f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShootBullets());
    }

    // Update is called once per frame
    void Update()
    {

        player = GameObject.FindGameObjectWithTag("Player");


       


        foreach (Transform gm in objects)
        {
            gm.transform.position = Target.transform.position;
        }


        Vector3 directionToPlayer = player.transform.position - transform.position;

        // Project the direction onto the horizontal plane (ignore y-axis)
        Vector3 horizontalDirection = new Vector3(directionToPlayer.x, 0f, directionToPlayer.z).normalized;

        // Calculate the rotation to look at the player only left and right
        Quaternion lookRotation = Quaternion.LookRotation(horizontalDirection);

        // Apply the rotation to the enemy, preserving the y-axis rotation
        transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
    }

    void Shoot()
    {
      
            // Instantiate and shoot each bullet
            InstantiateBullet(Point);
            InstantiateBullet(Point2);
        
    }

    void InstantiateBullet(Transform spawnPoint)
    {
        GameObject bullet = Instantiate(BulletPrefab, spawnPoint.position, spawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = bullet.transform.forward * ShootSpeed;
    }

    IEnumerator ShootBullets()
    {
        while (true)
        {
            // Shoot bullets
            for (int i = 0; i < numberOfShots; i++)
            {
                Shoot();
                yield return new WaitForSeconds(timeBetweenShots);
            }

            // Wait before shooting again
            yield return new WaitForSeconds(waitTime);
        }
    }

   
}
