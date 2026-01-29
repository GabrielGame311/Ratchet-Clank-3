using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NefouriusTrigger : MonoBehaviour
{
    public GameObject[] EnemiesRangers;
   
    public AudioSource sound;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {

            if (EnemiesRangers != null)
            {

                foreach (GameObject gm in EnemiesRangers)
                {



                     sound.Play();   
                   
                        gm.SetActive(true);
                    
                }
            }

            Nefourius.Nefourius_.Point++;
            Destroy(gameObject, 0.1f);

            

        }
    }
}
