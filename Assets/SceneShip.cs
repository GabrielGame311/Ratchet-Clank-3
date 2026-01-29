using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneShip : MonoBehaviour
{
    public GameObject timeline1;
    public GameObject timeline2;
    public GameObject cam;
    public Animator anime;
    public GameObject maincam;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        maincam.SetActive(false);
        anime.SetBool("Fade", true);
        yield return new WaitForSeconds(5.14f);
        timeline1.SetActive(false);
        timeline2.SetActive(true);
        yield return new WaitForSeconds(2.75f);
        anime.SetBool("Fade", false);
        timeline2.SetActive(false);
        cam.SetActive(false);
        maincam.SetActive(true);

    }

}
