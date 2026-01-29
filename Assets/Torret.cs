using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torret : MonoBehaviour
{
    
    
    public Transform Turret;

    public Animator anime;

    public Transform spawn;
   
    public float speed;
    public bool SeTheEnemie = false;

    public ParticleSystem particle;
    public AudioSource sound;
    public AudioClip soundplay;
    public AudioClip spawnsound;
    public AudioSource sound2;
    public GameObject enemie;
    public static Torret torrets;
    public  bool shoot = false;
    public float Distance;
    private int i = 0;


    public float TimeShoot = 1;
    float StartShoot;

    // Start is called before the first frame update
    void Start()
    {
        StartShoot = TimeShoot;
        torrets = GetComponent<Torret>();
        sound = GetComponent<AudioSource>();

        sound.PlayOneShot(spawnsound);
       

        sound2 = GameObject.FindGameObjectWithTag("Bolt").GetComponent<AudioSource>();
        
    }

    private void OnDestroy()
    {
        sound2.PlayOneShot(soundplay);
    }

    private void Awake()
    {
        StartCoroutine(DestorySound());


        
    }

    // Update is called once per frame
    void Update()
    {

        // Turret.transform.eulerAngles = new Vector3(0, Turret.transform.eulerAngles.y, 0);

       



     
        if(enemie != null)
        {
            SeTheEnemie = true;
        }
        else
        {
            SeTheEnemie = false;
        }



        

       


        if (SeTheEnemie)
        {

            TimeShoot -= Time.deltaTime;


            if (TimeShoot < 0)
            {
                Shooting();
                TimeShoot = StartShoot;
            }
            //spawn.transform.LookAt(enemie.transform);
            Turret.transform.LookAt(enemie.transform);
            shoot = true;
            anime.SetBool("Shoot", true);

           
           
        }
        else
        {
            anime.SetBool("Shoot", false);
            shoot = false;
        }
      
    }

    public void Shooting()
    {
        ParticleSystem prefab = Instantiate(particle, spawn.transform.position, spawn.transform.rotation);
        prefab.gameObject.GetComponent<Rigidbody>().velocity = -spawn.transform.forward * speed;
        Destroy(prefab.gameObject, 20);
        

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Sight")
        {

            enemie = other.gameObject;

            

          

            SeTheEnemie = true;


           
        }
       

        if (other.tag == "Box")
        {
            enemie = other.gameObject;

            SeTheEnemie = true;
        }
        if (other.tag == "AmmoBox")
        {
            enemie = other.gameObject;

            SeTheEnemie = true;
        }

    }

    
    

    void ParticlePlay()
    {
        
        
           // particle.Play();

        SeTheEnemie = true;


    }

    private void OnCollisionEnter(Collision collision)
    {
        anime.SetTrigger("Spawn");
        GetComponent<Rigidbody>().useGravity = false;
        GetComponent<Rigidbody>().isKinematic = true;

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Sight")
        {

            

            SeTheEnemie = false;
            //enemie = null;
          



        }
        
        if (other.tag == "Box")
        {
           

            SeTheEnemie = false;
        }
        if (other.tag == "AmmoBox")
        {
           

            SeTheEnemie = false;
        }
    }


    IEnumerator DestorySound()
    {
        yield return new WaitForSeconds(20);
        sound.PlayOneShot(soundplay);
    }
}
