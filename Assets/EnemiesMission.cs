using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemiesMission : MonoBehaviour
{
    public int EnemiesCount;
    public int NumberMusic;
    public bool IS = false;

    [Header("Mission Enemies")]
    [SerializeField] private List<GameObject> EnemiesList = new List<GameObject>();

    public static EnemiesMission instance;
    float loadsceneTime = 10;
    public bool SetactiveEnemies = false;
    public int Mission;
    public bool IsWin = false;
    GameObject player;
    bool isfalse = true;
    public string LoadScene;
    private bool hasProcessedEnemies = false;
    public bool SetactivePlayer = false;

    [HideInInspector]
    public bool isWaitingForNextWave = false;
    private int activeEnemiesCount = 0;

    void Start()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");

        // Om du har satt upp fiender i listan via Editorn, aktivera/inaktivera dem här baserat på din inställning
        if (SetactiveEnemies)
        {
            foreach (GameObject enemy in EnemiesList)
            {
                if (enemy != null) enemy.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        hasProcessedEnemies = false;
    }

    void Update()
    {
        // 1. Om listan är helt tom från början (inte inställd i Inspector), gör ingenting för att förhindra direkt vinst
        if (EnemiesList.Count == 0 && !hasProcessedEnemies)
        {
            return;
        }

        // 2. Nollställ räknaren för aktiva fiender denna frame
        activeEnemiesCount = 0;

        // 3. Gå igenom listan baklänges för att rensa bort helt förstörda (null) objekt
        for (int i = EnemiesList.Count - 1; i >= 0; i--)
        {
            if (EnemiesList[i] == null)
            {
                EnemiesList.RemoveAt(i);
            }
            else if (EnemiesList[i].activeSelf)
            {
                // Räkna endast fiender som är aktiva och lever i scenen just nu
                activeEnemiesCount++;
            }
        }

        // 4. VÅG-LOGIK: Trigga nästa spline när nuvarande aktiva våg är död (alla blivit inaktiverade/SetActive(false))
        if (activeEnemiesCount <= 0 && !isWaitingForNextWave && EnemiesList.Count > 0)
        {
            if (GalacticRangers.instance != null)
            {
                GalacticRangers.instance.AdvanceToNextSpline();
                isWaitingForNextWave = true; // Pausa signaler tills Rangers nått nästa spline-slut
            }
        }

        // 5. VINST-LOGIK: Trigga vinst ENDAST när hela listan är helt tom (alla fiender i hela uppdraget är Destroyed/null)
        if (EnemiesList.Count <= 0 && !IsWin)
        {
            IsWin = true;
        }

        // 6. HANTERA VINST OCH SCENBYTE
        if (IsWin)
        {
            if (isfalse)
            {
                Bolts.Bolt.BoltCount += MissionCompleteUI.MissionComplete.Bolts[MissionCompleteUI.MissionComplete.Mission];
                isfalse = false;
            }

            MissionCompleteUI.MissionComplete.gameObject.SetActive(true);
            loadsceneTime -= Time.deltaTime;

            if (loadsceneTime < 0)
            {
                
                
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                
                loadsceneTime = 10;
            }
        }

        // 7. ROCKET MISSION KONTROLLER
        
            MissionCompleteUI.MissionComplete.Mission = Mission;
            EnemiesCount = EnemiesList.Count;

            if (IS == false)
            {
                if (EnemiesList.Count <= 0)
                {
                    Bolts.Bolt.BoltCount += MissionCompleteUI.MissionComplete.Bolts[MissionCompleteUI.MissionComplete.Mission];
                    GetComponent<MissionSound>().i = NumberMusic;
                    GetComponent<MissionSound>().Mission4(NumberMusic);
                    MissionCompleteUI.MissionComplete.gameObject.SetActive(true);
                    IS = true;
                }
            }
            if(IS)
            {
                loadsceneTime -= Time.deltaTime;
                if (loadsceneTime < 0)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    loadsceneTime = 10;
                }
            }
           
        
    }

    // Publik funktion för att dynamiskt lägga till fiender till uppdraget från andra skript (t.ex. spawner-system)
    public void AddEnemyToMission(GameObject newEnemy)
    {
        if (newEnemy != null && !EnemiesList.Contains(newEnemy))
        {
            EnemiesList.Add(newEnemy);
            hasProcessedEnemies = true;
        }
    }
}