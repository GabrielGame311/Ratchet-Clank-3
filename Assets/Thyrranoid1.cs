using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thyrranoid1 : MonoBehaviour
{
    public GameObject ParticlePrefab;
    public float shootSpeed;
    public float MoveSpeed;
    Animator anime;


    public bool SePlayer = false;

    GameObject player;
    bool shoot = false;

    public float ShootTime;
    public Transform ParticlePoint1;
    public Transform particlePoint2;
    public float Distance = 30;
    float startshoot;
    public int ShootInt = 0;
    public float DistanceOfPlayer;
    public float minDistanceFromFirstEnemy;
    Vector3 pos;
    private Rigidbody rb;

    public float minHeight = -10f; 
    public float groundOffset = 0.1f;
    GameObject sightplayer;
    // Start is called before the first frame update
    void Start()
    {
        anime = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        startshoot = ShootTime;
        pos = transform.position;
        sightplayer = GameObject.FindGameObjectWithTag("SightPlayer");
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");

        float dis = Vector3.Distance(transform.position, player.transform.position);


        foreach (GameObject enemy in enemies)
        {


            float distances = Vector3.Distance(transform.position, enemy.transform.position);

            if (distances < minDistanceFromFirstEnemy)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                Vector3 newPosition = transform.position + direction * minDistanceFromFirstEnemy;
                enemy.transform.position = newPosition;
            }
        }

           
        
            if (dis <= Distance)
            {
                SePlayer = true;


                float disp = Vector3.Distance(transform.position, player.transform.position);
               

                if (DistanceOfPlayer < disp)
                {
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, MoveSpeed * Time.deltaTime);

                    //  transform.position = Vector3.t
                }






            }
            else
            {

                SePlayer = false;


            }
        

      

        if (SePlayer)
        {

          
            transform.LookAt(player.transform);
            //transform.position += Vector3.forward * MoveSpeed * Time.deltaTime;
            


            ShootTime -= Time.deltaTime;


           

        }
        else
        {
            transform.LookAt(pos);
            
        }


        if(shoot)
        {
            shoot = false;
             Shoot();
            anime.SetTrigger("Shoot");
        }
        else
        {

        }

        if (ShootTime < 0)
        {

            ShootTime = startshoot;
            shoot = true;
        }


       

    }

    private void FixedUpdate()
    {
        if (transform.position.y < minHeight)
        {
            // Move the enemy back to the ground
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
            {
                transform.position = hit.point + Vector3.up * groundOffset;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
    public void Shoot()
    {


       
            GameObject shoot = Instantiate(ParticlePrefab, ParticlePoint1.transform.position, ParticlePoint1.transform.rotation);
            shoot.GetComponent<Rigidbody>().linearVelocity = ParticlePoint1.transform.forward * shootSpeed;

            Destroy(shoot, 10);
            ParticlePoint1.transform.LookAt(sightplayer.transform);
            ShootInt++;
        
      

            GameObject shoot2 = Instantiate(ParticlePrefab, particlePoint2.transform.position, particlePoint2.transform.rotation);
            shoot2.GetComponent<Rigidbody>().linearVelocity = particlePoint2.transform.forward * shootSpeed;

            Destroy(shoot2, 10);

            particlePoint2.transform.LookAt(sightplayer.transform);
            ShootInt--;
        

    }

   
}
