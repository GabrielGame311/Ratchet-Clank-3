using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicDistance : MonoBehaviour
{


    public float Distance;
    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {


        float dis = Vector3.Distance(transform.position, player.transform.position);


        

        if(dis < Distance)
        {

            GameObject.FindObjectOfType<MusicPlay>().ChangeSound();
        }
        else
        {
            //GameObject.FindObjectOfType<MusicPlay>().ChangeSoundBack();
        }
       


    }
}
