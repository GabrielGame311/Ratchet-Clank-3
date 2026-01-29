using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoltGet : MonoBehaviour
{
    public Animator anime;
    public GameObject bolt;
    public GameObject hand;
    
    
    // Start is called before the first frame update
    void Start()
    {
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            anime.SetTrigger("GetBolt");
            bolt.SetActive(true);
           
        }
    }


    public void getbolt()
    {
        bolt.transform.parent = hand.transform;


    }

    public void disableparent()
    {
        bolt.transform.parent = null;
    }
}
