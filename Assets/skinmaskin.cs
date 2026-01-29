using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skinmaskin : MonoBehaviour
{

    public GameObject smoke;
    public GameObject camera;
    private Animator anime;
    public GameObject timeline1;
    public GameObject canvas;


    public static bool ISTrue = false;
    
    // Start is called before the first frame update
    void Start()
    {
        anime = GetComponent<Animator>();
    }


   

    private void OnTriggerStay(Collider other)
    {
        
        
            
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                

                canvas.SetActive(true);

                

                Time.timeScale = 0f;

                Cursor.lockState = CursorLockMode.None;



            }
            else
            {
               Cursor.lockState = CursorLockMode.Locked;
               Time.timeScale = 1f;
                canvas.SetActive(false);
                anime.SetBool("closed", false);
                anime.SetBool("open", true);

            }
        
        
            
       
        
       
        
    }

    





    public void BuySkin()
    {
        StartCoroutine(timeline());
    }



    IEnumerator timeline()
    {
        
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        camera.SetActive(true);
        timeline1.SetActive(true);
        anime.SetBool("closed", false);
        anime.SetBool("open", true);

        smoke.SetActive(true);
        yield return new WaitForSeconds(4);

        smoke.SetActive(false);
        yield return new WaitForSeconds(1.11f);
        timeline1.SetActive(false);
        camera.SetActive(false);
       

    }

}
