using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{

    public float time;

    public bool Veldin = false;
    public bool Aquatos = false;
    public bool ClankSectionAquatos = false;

    private void Start()
    {
        StartCoroutine(wait());
    }


    private void Update()
    {
       
        //Skiping The Scene;
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (Aquatos == true)
            {
               

                SceneManager.LoadScene("Aquatos");
            }

            if (Veldin == true)
            {
               

                SceneManager.LoadScene("Veldins");
            }

            if (ClankSectionAquatos == true)
            {
                

                SceneManager.LoadScene("ClankAquatos");
            }

        }
    }




    IEnumerator wait()
    {
        
        if(Aquatos == true)
        {
            yield return new WaitForSeconds(time);

            SceneManager.LoadScene("Aquatos");
        }
        
        if (Veldin == true)
        {
            yield return new WaitForSeconds(time);

            SceneManager.LoadScene("Veldins");
        }

        if (ClankSectionAquatos == true)
        {
            yield return new WaitForSeconds(time);

            SceneManager.LoadScene("ClankAquatos");
        }



    }





}
