using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo_Pickup : MonoBehaviour
{

    bool Pikup = false;

    public float speed;
    public GameObject player;
    Rigidbody rb;

    public int AmmoCount;
    public float Distance;
   
    bool touchobject = false;

    private AudioSource sound;
    public AudioClip soundeffects;

    

    // Start is called before the first frame update
    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Bolt");
        rb = GetComponent<Rigidbody>();

        sound = GameObject.FindGameObjectWithTag("Bolt").GetComponent<AudioSource>();


    }

    // Update is called once per frame
    void Update()
    {

        if(GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo < GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().MaxAmmo)
        {
            if (Pikup == true)
            {
               

                float dis = Vector3.Distance(transform.position, player.transform.position);

                if(dis < Distance)
                {
                    float step = speed * Time.deltaTime;

                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, step);


                }
            }
        }
        
           
        
      


    }

    private void OnCollisionEnter(Collision collision)
    {
        Pikup = true;
    }

    private void OnTriggerEnter(Collider other)
    {









      

        
       
        
        if (other.CompareTag("Bolt"))
        {

             
                GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo += AmmoCount;
            


                sound.PlayOneShot(soundeffects);
               
             
               


            Destroy(gameObject);
            




        }



    }


}
