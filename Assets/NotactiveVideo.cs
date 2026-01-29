using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;



public class NotactiveVideo : MonoBehaviour
{

    public GameObject MainMenu_;

    VideoPlayer Player_;


    // Start is called before the first frame update
    void Start()
    {
        Player_ = GetComponent<VideoPlayer>();
        Player_.loopPointReached += OnVideoEnd;
    }

    // Update is called once per frame
    void Update()
    {
       

    }

   

    void OnVideoEnd(VideoPlayer vp)
    {
        gameObject.SetActive(false);
        MainMenu_.SetActive(true);
    }


}
