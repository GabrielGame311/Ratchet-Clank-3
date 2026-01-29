using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{

    bool Pikup = false;

    public float speed;
    public GameObject player;
    Rigidbody rb;

    public int boltCount;

    bool touchobject = false;
    
    private AudioSource sound;
    public AudioClip soundeffects;

   
    
    // Start is called before the first frame update
    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Bolt");
        rb = GetComponent<Rigidbody>();

        sound = GetComponentInParent<AudioSource>();
       
       
    }

    // Update is called once per frame
    void Update()
    {


        float dis = Vector3.Distance(transform.position, player.transform.position);

        if(dis < 15)
        {
            Pikup = true;
        }


        if(Pikup == true)
        {
            

            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        

       
          
        


       
        
            if (other.CompareTag("Player"))
            {

                Pikup = true;
                Bolts.Bolt.CountTime = 2;

                sound.PlayOneShot(soundeffects);
                Bolts.Bolt.BoltCount += boltCount;

                Destroy(gameObject);

            }

          

       
    }

    

   
}
