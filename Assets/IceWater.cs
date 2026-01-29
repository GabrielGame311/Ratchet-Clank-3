using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IceWater : MonoBehaviour
{


    public GameObject IceObject;
   

   

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Playerholder")
        {
           // player.GetComponent<CharacterController>().enabled = false;
            IceObject.SetActive(true);
            StartCoroutine(waitDie());
        }
    }



    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Player")
        {
            IceObject.SetActive(true);
            StartCoroutine(waitDie());
        }
    }

    IEnumerator waitDie()
    {

        yield return new WaitForSeconds(5);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
}
