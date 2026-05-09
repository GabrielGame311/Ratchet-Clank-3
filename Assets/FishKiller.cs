using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class FishKiller : MonoBehaviour
{

    Animator anime;

    public float Impuls;
    public float rotate;
    Rigidbody rb;
    public AudioClip Soundfx;
    AudioSource sound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sound = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        anime = GetComponentInChildren<Animator>();
        StartCoroutine(wait());
        rb.AddForce(transform.forward * Impuls);

    }

    // Update is called once per frame
    void Update()
    {

        




    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ratchet")
        {

            other.gameObject.SetActive(false);
        }
    }

    IEnumerator wait()
    {

        yield return new WaitForSeconds(0);

        anime.SetTrigger("Eat");
        sound.PlayOneShot(Soundfx);
        GameObject.FindObjectOfType<Player>().TakeDamage(250);
        GameObject.FindObjectOfType<Player>().GetComponent<CharacterController>().enabled = false;
        yield return new WaitForSeconds(1);
        transform.Rotate(rotate, 0, 0);
       
        yield return new WaitForSeconds(1);
        
        Destroy(gameObject);

    }
}
