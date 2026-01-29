using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Aquatos_shooting : MonoBehaviour
{
    
    public Animator anime;
    public float shootingSpeed;
   
    public int damage;
    
    public Transform gun;
    public bool shooting;
    private SphereCollider collidier;
   
    private Transform player;
   

    public Bridge bridge;

   

    private EnemiesHealth health;

    bool canattack = false;

    public GameObject crackedObject;
    private GameObject objects;

    public  bool shootparticle = false;
    public ParticleSystem particleshoot;
    
    public Transform spawn;

    private Vector3 direction;
    private Quaternion rotation;
    public float ParticleSpeed;

    bool isspawn = true;

    

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        collidier = GetComponent<SphereCollider>();


        health = GetComponent<EnemiesHealth>();

        objects = GetComponent<GameObject>();
       
    }

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {



       

        if(isspawn == true)
        {
            if (health.health == 0)
            {
                Instantiate(crackedObject, transform.position, transform.rotation);

                isspawn = false;










            }
        }

        if (shooting == true)
        {

           



            transform.LookAt(player);

           

           

           


            if (shootparticle == true)
            {
                anime.SetBool("Shooting", true);
               
               
            }



        }


       

        if (shooting == false)
        {
            

            anime.SetBool("Shooting", false);
        }

    }

  
   



    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            shootparticle = true;
            shooting = true;
            collidier.enabled = false;
          
            



        }
    }

   

   

    

    
}
