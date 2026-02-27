using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class BattleMission_UI : MonoBehaviour
{
    public string LoadScene = "Thyrra_Mission";
    public int MissionsList = 0;
    public static BattleMission_UI BattleMission;
    public GameObject InGame_;
    public Sprite[] MissionImage;
    public Image Image;
    public int selected = 0;
    public GameObject MissionShip;
    public GameObject Player_;
    
    // Start is called before the first frame update
    void Start()
    {
        BattleMission = GetComponent<BattleMission_UI>();
        Cursor.lockState = CursorLockMode.None;


    }

    // Update is called once per frame
    void Update()
    {



        if (selected == 0)
        {


            Image.sprite = MissionImage[0];

        }
        if (selected == 1)
        {

            Image.sprite = MissionImage[1];

        }

        if (selected == 2)
        {

            Image.sprite = MissionImage[2];

        }
        if (selected == 3)
        {

            Image.sprite = MissionImage[3];

        }

        if (MissionsList == 0)
        {

            Missions.Mission_.Mission = 0;
            MissionImage[0] = Image.sprite;

        }
        if(MissionsList == 1)
        {
            Missions.Mission_.Mission = 1;
            MissionImage[1] = Image.sprite;

        }

        if (MissionsList == 2)
        {
            Missions.Mission_.Mission = 2;
            MissionImage[2] = Image.sprite;

        }
        if (MissionsList == 3)
        {
            Missions.Mission_.Mission = 3;
            MissionImage[3] = Image.sprite;

        }


        





    }

    public void SelectImg(int select)
    {
        selected = select;





    }


    public void Mission(int mission)
    {

        MissionsList = mission;

        if(LoadScene != null)
        {
            SceneManager.LoadScene(LoadScene);
        }
        MissionMap(MissionsList);
        InGame_.SetActive(true);
        Player_.SetActive(true);
        MissionShip.SetActive(false);
        gameObject.SetActive(false);


        

    }

    public void MissionMap(int missions)
    {

        MissionsList = missions;
        // GameObject.FindObjectOfType<Missions>().Mission = missions;

        GameObject.FindObjectOfType<Missions>().MissionsSet(missions);
    }
}
