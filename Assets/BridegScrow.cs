using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridegScrow : MonoBehaviour
{

    public GameObject BrideLeft;
    public GameObject BrideRight;
    public float BridgeSpeed;
    public float ScrowSpeed;
    bool IsTrigger = false;
    Quaternion rot;
    Vector3 pos;
    bool IsScrow = false;
    public Animator anime;
    public GameObject Enemies;
    bool isCrowed = false;
    public static BridegScrow BridgeScrow_;
    public GameObject IOS_UI;
    public bool Scrowing = false;

    float count;

    // Start is called before the first frame update
    void Start()
    {
        rot = transform.rotation;
        pos = transform.position;
        BridgeScrow_ = GetComponent<BridegScrow>();

        Enemies.SetActive(false);

        count = 287.23f;
    }

    // Update is called once per frame
    void Update()
    {

        if (Scrowing && transform.position.y > 287.23f)
        {
            transform.position -= Vector3.up * ScrowSpeed * Time.deltaTime;

          //  anime.SetBool("Forward", true);
            count -= 50 * Time.deltaTime;
            // Update your UI slider value if needed.
             IOSController.IosController_.Slider_.value = count;
        }


        if (IsTrigger)
        {

           // IOS_UI.SetActive(true);


            if(IsScrow == false)
            {

                


                if (Input.GetKey(KeyCode.E))
                {

                    if (transform.position.y > 287.23f)
                    {
                        transform.position -= Vector3.up * ScrowSpeed * Time.deltaTime;

                        //anime.SetBool("Forward", true);
                       
                    }

                 

                   
                }
                else
                {
                    //if (transform.position.y < pos.y)
                   // {
                       // transform.position += Vector3.up * ScrowSpeed * Time.deltaTime;
                      //  anime.SetBool("Forward", false);
                    //}







                }


                if(transform.position.y < 287.23f)
                {
                    isCrowed = true;
                }
            }

          


        }
        else
        {

            IOS_UI.SetActive(false);

        }

        if (transform.position.y < 287.23f)
        {
            IsScrow = true;
           

        }

        if(isCrowed)
        {


            // Enemies.SetActive(true);
            //  IOS_UI.SetActive(false);
            GameObject.FindObjectOfType<SpawnTime>().RangerTalk();

            if(MissionSound.MissionSound_ != null)
            {
                MissionSound.MissionSound_.i++;
                MissionSound.MissionSound_.Mission4(MissionSound.MissionSound_.i);
            }





            isCrowed = false;
        }


    }




    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {

            IsTrigger = true;


        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {

            IsTrigger = false;


        }




    }
}
