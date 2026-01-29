using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{

    Animator anime;
    public string AnimeName;
    public Animator animeTrigger;
    // Start is called before the first frame update
    void Start()
    {
        anime = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(animeTrigger != null)
            {
                animeTrigger.SetTrigger(AnimeName);
            }
            anime.SetTrigger(AnimeName);
            Destroy(GetComponent<BoxCollider>());
        }
    }
}
