using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketShooter2 : MonoBehaviour
{

    public float ShootTime = 3;

    float startTime;

    public GameObject Bullet;
    public float BulletSpeed = 10;
    public Transform BulletPoint;
    
    public Transform Head;
    public float Distance = 30;
    bool SePlayer = false;
    GameObject player;
    Animator anime;

    GameObject prefablist;

    // Start is called before the first frame update
    void Start()
    {
        anime = GetComponent<Animator>();
        
        startTime = ShootTime;
    }

    // Update is called once per frame
    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        float dis = Vector3.Distance(transform.position, player.transform.position);


        if(dis < Distance)
        {
            SePlayer = true;
        }
        else
        {
            SePlayer = false;
        }



        if(SePlayer)
        {

            
            Head.transform.LookAt(player.transform);
            anime.SetBool("Se", true);
            ShootTime -= Time.deltaTime;


            if (ShootTime < 0)
            {

                ShootTime = startTime;

                Shoot();



            }
        }
        else
        {
            anime.SetBool("Se", false);
        }

      


    }

    private void FixedUpdate()
    {


          //  prefablist.transform.LookAt(player.transform);

       // prefablist.transform.Translate(Vector3.forward * 10* Time.deltaTime);
       // Vector3.MoveTowards(prefablist.transform.position, player.transform.position, Time.deltaTime * 5);


    }

    void Shoot()
    {
        GameObject prefab = Instantiate(Bullet, BulletPoint.transform.position, BulletPoint.transform.rotation);

        prefab.GetComponent<Rigidbody>().velocity = BulletPoint.transform.forward * BulletSpeed;

        //prefab.GetComponent<Rigidbody>().useGravity = true;
          prefablist = prefab;

	      
        

    }

   
    





}
