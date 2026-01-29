using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NormalCutscene : MonoBehaviour
{

    public GameObject cutscenetimeline;
    public GameObject CutsceneCam;
    public float CutsceneDuration;
    
    public AudioSource Music1;
    public Animator anime;
    public Animator animeside;
    public GameObject timeline2;
    public GameObject trigger;

    public bool Scene = true;

    public string LoadScene;

    bool isTrigger = false;
    bool IsPressed = false;

    private void Update()
    {
        if(!Scene)
        {
            if(isTrigger)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {




                    if(IsPressed)
                    {
                        GameObject.FindObjectOfType<AllGameData>().EnablePlayerDo();
                        GameObject.FindObjectOfType<MusicPlay>().AudioSource_.Play();
                        IsPressed = false;
                        QuarkMenuComics.Instance.QuarkComicMenu_.SetActive(false);
                        Cursor.lockState = CursorLockMode.Locked;
                        Time.timeScale = 1;
                        IsPressed = false;
                    }
                    else
                    {
                        GameObject.FindObjectOfType<AllGameData>().DisablePlayerDo();
                        QuarkMenuComics.Instance.QuarkComicMenu_.SetActive(true);
                        Cursor.lockState = CursorLockMode.None;
                        Time.timeScale = 0;
                        GameObject.FindObjectOfType<MusicPlay>().AudioSource_.Pause();
                        IsPressed = true;
                    }
                }
            }
           
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {

            isTrigger = true;

           if(Scene == true)
           {
                anime = GameObject.Find("fade").GetComponent<Animator>();
                anime.SetBool("Fade", true);
                CutsceneCam.SetActive(true);
                cutscenetimeline.SetActive(true);
                Music1.Pause();
                StartCoroutine(wait());
           }
            
            if(Scene == false)
            {
                timeline2.SetActive(true);
            }
            
        }         
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            isTrigger = false;
        }
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(CutsceneDuration);
        anime.SetBool("Fade", false);
        if(LoadScene != null)
        {
            SceneManager.LoadScene(LoadScene);
        }
        cutscenetimeline.SetActive(false);
        CutsceneCam.SetActive(false);
        Music1.Play();
        timeline2.SetActive(true);
        animeside.SetTrigger("Hide");
        trigger.SetActive(false);
    }

}
