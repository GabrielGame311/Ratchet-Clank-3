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
    public bool ISEnemiesAdd = false;
    void Start()
    {
        instance = this;
        player = GameObject.FindGameObjectWithTag("Player");

        // Om du har satt upp fiender i listan via Editorn, aktivera/inaktivera dem h�r baserat p� din inst�llning
        if (SetactiveEnemies)
        {
            foreach (GameObject enemy in EnemiesList)
            {
                if (enemy != null) enemy.SetActive(false);
            }
        }
        if(ISEnemiesAdd)
        {
            FindAllEnemiesIncludeInactive();
            ISEnemiesAdd = false;
        }
    }

    private void OnEnable()
    {
        hasProcessedEnemies = false;
    }

    void Update()
    {



        

        // 1. Om listanr helt tom frn brjan (inte instlld i Inspector), gr ingenting fr att frhindra direkt vinst
        if (EnemiesList.Count == 0 && !hasProcessedEnemies)
        {
            return;
        }

        // 2. Nollst�ll r�knaren f�r aktiva fiender denna frame
        activeEnemiesCount = 0;

        // 3. G� igenom listan bakl�nges f�r att rensa bort helt f�rst�rda (null) objekt
        for (int i = EnemiesList.Count - 1; i >= 0; i--)
        {
            if (EnemiesList[i] == null)
            {
                EnemiesList.RemoveAt(i);
            }
            else if (EnemiesList[i].activeSelf)
            {
                // R�kna endast fiender som �r aktiva och lever i scenen just nu
                activeEnemiesCount++;
            }
        }

        // 4. V�G-LOGIK: Trigga n�sta spline n�r nuvarande aktiva v�g �r d�d (alla blivit inaktiverade/SetActive(false))
        if (activeEnemiesCount <= 0 && !isWaitingForNextWave && EnemiesList.Count > 0)
        {
            if (GalacticRangers.instance != null)
            {
                GalacticRangers.instance.AdvanceToNextSpline();
                isWaitingForNextWave = true; // Pausa signaler tills Rangers n�tt n�sta spline-slut
            }
        }

        // 5. VINST-LOGIK: Trigga vinst ENDAST n�r hela listan �r helt tom (alla fiender i hela uppdraget �r Destroyed/null)
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

    void FindAllEnemiesIncludeInactive()
    {
        EnemiesList.Clear();

        // Hämta alla Transforms i scenen (även de som är inaktiva)
        Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Transform t in allTransforms)
        {
            if (t.CompareTag("Enemie")) // Kontrollera taggen
            {
                EnemiesList.Add(t.gameObject);
            }
        }
    }
    // Publik funktion f�r att dynamiskt l�gga till fiender till uppdraget fr�n andra skript (t.ex. spawner-system)
    public void AddEnemyToMission(GameObject newEnemy)
    {
        if (newEnemy != null && !EnemiesList.Contains(newEnemy))
        {
            EnemiesList.Add(newEnemy);
            hasProcessedEnemies = true;
        }
    }
}