using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPhinix : MonoBehaviour
{
    public string AnimeName;
    public Animator animeTrigger;
    // Start is called before the first frame update

    public AudioSource sound;

    public AudioClip SoundEffect;
    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (animeTrigger != null)
            {
                animeTrigger.SetBool(AnimeName, true);
                sound.PlayOneShot(SoundEffect);
            }
            
           
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (animeTrigger != null)
            {
                animeTrigger.SetBool(AnimeName, false);
                sound.PlayOneShot(SoundEffect);
            }


        }
    }
}
