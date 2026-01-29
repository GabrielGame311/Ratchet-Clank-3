using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuarkMenuComics : MonoBehaviour
{

    public GameObject QuarkComicMenu_;
    public static QuarkMenuComics Instance;

    public string GameComic;


    // Start is called before the first frame update
    void Start()
    {
        Instance = this;



    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void VidComic(string comics)
    {
        GameComic = comics;
        SceneManager.LoadScene(comics);

    }

}
