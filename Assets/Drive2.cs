using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drive2 : MonoBehaviour
{
    
    public GameObject player;
    public GameObject timeline;
    public GameObject His;
    public float time;
    private RatchetController movment;
    public GameObject character;
    public GameObject ch2;

    public CharacterController controller;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        movment = RatchetController.FindObjectOfType<RatchetController>(tag == "Player");

        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
        
    }

    private void OnTriggerEnter(Collider other)
    { 
        if(other.tag == "Ratchet")
        {
            character.transform.SetParent(His.transform);
            player.transform.SetParent(His.transform);
            movment.enabled = false;
            timeline.SetActive(true);
            character.SetActive(true);
            ch2.SetActive(false);
            controller.enabled = false;
            StartCoroutine(wait());

        }

        
        
            
        
    }


    IEnumerator wait()
    {
        yield return new WaitForSeconds(time);
        player.transform.SetParent(null);
        character.transform.SetParent(null);
        movment.enabled = true;
        character.SetActive(false);
        controller.enabled = true;
    }

}
