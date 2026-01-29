using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour
{


    public GameObject ball;

    public GameObject torret;

    public AudioSource sound;
    public AudioClip soundclip;

    // Start is called before the first frame update
    void Start()
    {
        ball = GetComponent<GameObject>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnDestroy()
    {  GameObject toretts = Instantiate(torret, transform.position, Quaternion.identity);
        Destroy(toretts, 20);

        if(toretts)
        {
            sound.PlayOneShot(soundclip);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {

        
       //var toretts = GameObject.Instantiate(torret, ball.transform.position, Quaternion.identity);
      

        // Destroy(toretts, 20);

       
            
            Destroy(gameObject);
        

       
    }


    
    private void OnTriggerEnter(Collider other)
    {
       
       

        
    }
}
