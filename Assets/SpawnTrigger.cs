using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{

    public Transform SpawnPoint;
    public GameObject DisableEneimes;
    public AudioSource music;
    public AudioClip MusicClip;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Destroy(DisableEneimes);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {

            other.GetComponentInChildren<CharacterController>().enabled = false;
            other.gameObject.transform.position = SpawnPoint.transform.position;
            other.GetComponentInChildren<CharacterController>().enabled = true;

            music.clip = MusicClip;
            music.Play();
            Destroy(gameObject, 2);
        }
    }

}
