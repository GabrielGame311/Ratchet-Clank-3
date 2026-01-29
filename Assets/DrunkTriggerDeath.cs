using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class DrunkTriggerDeath : MonoBehaviour
{

    Animator fade;
    GameObject player;
    public string LoadScene;

    // Start is called before the first frame update
    void Start()
    {
        fade = GameObject.Find("fade").GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Ratchet");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ratchet")
        {
            player.GetComponent<Animator>().SetTrigger("Drunk");
            player.GetComponentInParent<Player>().TakeDamage(100);
            
            StartCoroutine(waitDie());
        }


        if(other.tag == "Enemie")
        {
           // other.GetComponent<EnemiesHealth>().TakeDamage(101);
        }
    }

    IEnumerator waitDie()
    {
        yield return new WaitForSeconds(5);

        fade.SetBool("Fade", true);
        yield return new WaitForSeconds(1.2f);

        SceneManager.LoadScene(LoadScene);
    }
}
