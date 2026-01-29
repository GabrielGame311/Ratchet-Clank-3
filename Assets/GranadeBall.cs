using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranadeBall : MonoBehaviour
{

    public GameObject particle2;
    public float rotateSpeed;
    public AudioSource sound;
    public AudioClip soundexp;
    
    // Start is called before the first frame update
    void Start()
    {
        sound = GameObject.FindGameObjectWithTag("Bolt").GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        Instantiate(particle2, transform.position, transform.rotation);
        sound.PlayOneShot(soundexp);

        GameObject.FindObjectOfType<GranadeShooter>().shoot = true;
    }


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(+rotateSpeed, 0, +rotateSpeed);
    }


    private void OnCollisionEnter(Collision collision)
    {
        
        Destroy(gameObject);
        
    }



    
}
