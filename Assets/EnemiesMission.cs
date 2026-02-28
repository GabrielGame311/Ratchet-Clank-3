using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class EnemiesMission : MonoBehaviour
{

    public EnemiesHealth[] Enemies;
    public int EnemiesCount;
    public int NumberMusic;
    bool IS = false;
    public List<GameObject> EnemiesList = new List<GameObject>();
    public static EnemiesMission instance;
    float loadsceneTime = 10;
    public bool SetactiveEnemies = false;
    public int Mission;
    public bool IsWin = false;
    GameObject player;
    bool isfalse = true;
    public string LoadScene;

    public bool SetactivePlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        instance = GetComponent<EnemiesMission>();

        Enemies = FindObjectsOfType<EnemiesHealth>();
        player = GameObject.FindGameObjectWithTag("Player");

        if(gameObject.activeSelf == true)
        {
            foreach (EnemiesHealth enemy in Enemies)
            {
                EnemiesList.Add(enemy.gameObject);


                if (SetactiveEnemies)
                {
                    enemy.gameObject.SetActive(false);
                }
            }
        }

      







    }

   

    // Update is called once per frame
    void Update()
    {

        if (IsWin)
        {







            if (isfalse)
            {
                Bolts.Bolt.BoltCount += MissionCompleteUI.MissionComplete.Bolts[MissionCompleteUI.MissionComplete.Mission];
                isfalse = false;
            }
            // GetComponent<MissionSound>().i = NumberMusic;
            //GetComponent<MissionSound>().Mission4(NumberMusic);
            MissionCompleteUI.MissionComplete.gameObject.SetActive(true);

            loadsceneTime -= Time.deltaTime;

            if (loadsceneTime < 0)
            {

                if (LoadScene != null)
                {
                    SceneManager.LoadScene(LoadScene);
                }
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }


                loadsceneTime = 10;
            }
        }
        else
        {

        }


        if (GetComponent<RocketMission>() != null)
        {

            MissionCompleteUI.MissionComplete.Mission = Mission;

            // enemies = Enemies;



            EnemiesCount = EnemiesList.Count;

            if (IS == false)
            {
                if (EnemiesCount == 0)
                {
                    Bolts.Bolt.BoltCount += MissionCompleteUI.MissionComplete.Bolts[MissionCompleteUI.MissionComplete.Mission];
                    GetComponent<MissionSound>().i = NumberMusic;
                    GetComponent<MissionSound>().Mission4(NumberMusic);
                    MissionCompleteUI.MissionComplete.gameObject.SetActive(true);

                    IS = true;
                }

            }
            else
            {
                loadsceneTime -= Time.deltaTime;

                if (loadsceneTime < 0)
                {

                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    loadsceneTime = 10;
                }


            }

        }



        

       
    }
}
