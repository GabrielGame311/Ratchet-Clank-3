using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{

    public GameObject TimelineScene;
    public RatchetController player;
    public Animator playerobj;
    public float TimelineTimer = 15f;
    public rangerwalkanime rangerswalk;
    public BoxCollider box;
    private Cutscene cutscene2;
    public Animator anime;
    public AudioSource MusicMap;
    public AudioSource MusicMap2;
    public Camera MainCam;
    public GameObject SceneCam;
    public GameObject afterscene;
    public Animator animeranger;
    private bool IsSkiping = false;
    public GameObject enemies;
    private void Start()
    {
        cutscene2 = GetComponent<Cutscene>();


        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<RatchetController>();
        box = GetComponent<BoxCollider>();
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
           
            StartCoroutine(time());

        }
       
        

    }

    private void Update()
    {
        
        if(IsSkiping == true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                IsSkiping = false;
                TimelineScene.SetActive(false);
                player.enabled = true;
                box.enabled = false;
                cutscene2.enabled = false;
                MusicMap.Play();
                MusicMap2.Play();
                anime.SetBool("Fade", false);
                MainCam.enabled = true;
                playerobj.applyRootMotion = true;
                SceneCam.SetActive(false);
                afterscene.SetActive(true);
                rangerswalk.enabled = true;
                enemies.SetActive(true);
            }
        }
        
        
    }


    IEnumerator time()
    {
        IsSkiping = true;
        playerobj.applyRootMotion = false;
        MusicMap.Pause();
        MusicMap2.Pause();
        anime.SetBool("Fade", true);
        SceneCam.SetActive(true);
        MainCam.enabled = false;
        yield return new WaitForSeconds(1);

        player.enabled = false;
        TimelineScene.SetActive(true);
        yield return new WaitForSeconds(TimelineTimer);
        anime.SetBool("Fade", false);
        yield return new WaitForSeconds(0.5f);
       
        TimelineScene.SetActive(false);
        player.enabled = true;
        box.enabled = false;
        cutscene2.enabled = false;
        MusicMap.Play();
        MusicMap2.Play();
        MainCam.enabled = true;
        playerobj.applyRootMotion = true;
        SceneCam.SetActive(false);
        afterscene.SetActive(true);
        rangerswalk.enabled = true;
        animeranger.SetTrigger("Walk");
        enemies.SetActive(true);
    }
}
