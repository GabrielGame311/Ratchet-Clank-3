using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterClank : MonoBehaviour
{

    public float SpeedFollow;

    public float shootSpeed;

    public GameObject Particle;
    public Transform ShootPoint;
    Vector3 StartPos;
    public bool IsTrigger = false;
    public bool IsShoot = false;
    GameObject Player;
    Animator anime;
    public float ShootTime;
    float startShoot;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Clank");
        anime = GetComponentInChildren<Animator>();
        StartPos = transform.position;
        startShoot = ShootTime;

    }

    // Update is called once per frame
    void Update()
    {
        if(IsTrigger)
        {
            anime.SetBool("Idle", false);
            StartCoroutine(wait());
        }
        else
        {
            Vector3 currentPosition = transform.position;

            // H�mta spelarens X-position men beh�ll objektets nuvarande Y- och Z-position
            Vector3 targetPosition = new Vector3(currentPosition.x, currentPosition.y, StartPos.z);

            // Flytta objektet mot spelarens X-position
            transform.position = Vector3.MoveTowards(currentPosition, targetPosition, SpeedFollow * Time.deltaTime);
            if(transform.position == StartPos)
            {
                anime.SetBool("Idle", true);
            }
        }

       
    }




    IEnumerator wait()
    {
        ShootTime -= Time.deltaTime;
        if(ShootTime < 0)
        {
            ShootTime = startShoot;

            Shoot();
        }
        
        Vector3 currentPosition = transform.position;

        // H�mta spelarens X-position men beh�ll objektets nuvarande Y- och Z-position
        Vector3 targetPosition = new Vector3(currentPosition.x, currentPosition.y, Player.transform.position.z);

        // Flytta objektet mot spelarens X-position
        transform.position = Vector3.MoveTowards(currentPosition, targetPosition, SpeedFollow * Time.deltaTime);


        


        yield return new WaitForSeconds(15);

        IsTrigger = false;

    }

    public void Shoot()
    {
        GameObject obj = Instantiate(Particle, ShootPoint.transform.position, ShootPoint.transform.rotation);

        obj.GetComponent<Rigidbody>().linearVelocity = ShootPoint.transform.forward * shootSpeed;

        Destroy(obj, 4);
    }
}
