using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Transform enemyParent;

    public List<ScrewMission> ScrewMission_ = new List<ScrewMission>();

    void Start()
    {
        instance = this;

        if (ScrewMission_ == null) ScrewMission_ = new List<ScrewMission>();
        ScrewMission_.AddRange(FindObjectsOfType<ScrewMission>());

        player = GameObject.FindGameObjectWithTag("Player");

        if (SetactiveEnemies)
        {
            foreach (GameObject enemy in EnemiesList)
            {
                if (enemy != null) enemy.SetActive(false);
            }
        }

        if (ISEnemiesAdd)
        {
            FindAllEnemiesIncludeInactive();
            ISEnemiesAdd = false;
        }

        // MARKEN FÖR UPPDRAGSSTART:
        // Om det finns fiender eller skruv-uppdrag vid start markerar vi att uppdraget är igång!
        if (EnemiesList.Count > 0 || ScrewMission_.Count > 0)
        {
            hasProcessedEnemies = true;
        }
    }

    private void OnEnable()
    {
        hasProcessedEnemies = false;
    }

    void Update()
    {
        // 1. Om uppdraget inte har startats ännu (listorna var tomma från början), förhindra vinst på Frame 1
        if (!hasProcessedEnemies)
        {
            if (EnemiesList.Count > 0 || ScrewMission_.Count > 0)
            {
                hasProcessedEnemies = true;
            }
            else
            {
                return; // Vänta tills uppdraget får objekt
            }
        }

        // 2. RENSA SKRUVMISSIONER (Ta bort klara/inaktiverade skruv-uppdrag)
        for (int i = ScrewMission_.Count - 1; i >= 0; i--)
        {
            if (ScrewMission_[i] == null || ScrewMission_[i].isScrowed || !ScrewMission_[i].enabled)
            {
                ScrewMission_.RemoveAt(i);
            }
        }

        // 3. Nollställ räknaren för aktiva fiender denna frame
        activeEnemiesCount = 0;

        // 4. Gå igenom fiendelistan baklänges för att rensa bort förstörda (null) objekt
        for (int i = EnemiesList.Count - 1; i >= 0; i--)
        {
            if (EnemiesList[i] == null)
            {
                EnemiesList.RemoveAt(i);
            }
            else if (EnemiesList[i].activeSelf)
            {
                activeEnemiesCount++;
            }
        }

        // 5. VÅG-LOGIK: Trigga nästa spline när BÅDE fiender och skruvuppdrag är tomma
        if (activeEnemiesCount <= 0 && !isWaitingForNextWave && EnemiesList.Count == 0 && ScrewMission_.Count == 0)
        {
            if (GalacticRangers.instance != null)
            {
                GalacticRangers.instance.AdvanceToNextSpline();
                isWaitingForNextWave = true;
            }
        }

        // 6. VINST-LOGIK: Trigga vinst när alla fiender OCH skruvuppdrag är borta
        if (EnemiesList.Count == 0 && ScrewMission_.Count == 0 && !IsWin)
        {
            IsWin = true;
        }

        // 7. HANTERA VINST OCH SCENBYTE
        if (IsWin)
        {
            if (isfalse)
            {
                Bolts.Bolt.BoltCount += MissionCompleteUI.MissionComplete.Bolts[MissionCompleteUI.MissionComplete.Mission];
                isfalse = false;
            }

            MissionCompleteUI.MissionComplete.gameObject.SetActive(true);
            loadsceneTime -= Time.deltaTime;

            if (loadsceneTime < 0 && !GetComponent<MissionSound>().sound.isPlaying)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                loadsceneTime = 10;
            }
        }

        // 8. ROCKET MISSION KONTROLLER
        MissionCompleteUI.MissionComplete.Mission = Mission;
        EnemiesCount = EnemiesList.Count;

        if (IS == false)
        {
            if (EnemiesList.Count <= 0 && ScrewMission_.Count == 0)
            {
                Bolts.Bolt.BoltCount += MissionCompleteUI.MissionComplete.Bolts[MissionCompleteUI.MissionComplete.Mission];
                
                MissionSound missionSound = GetComponent<MissionSound>();
                if (missionSound != null)
                {
                    missionSound.i = NumberMusic;
                    missionSound.Mission4(NumberMusic);
                }

                MissionCompleteUI.MissionComplete.gameObject.SetActive(true);
                IS = true;
            }
        }

        if (IS)
        {
            loadsceneTime -= Time.deltaTime;
            if (loadsceneTime < 0 && !GetComponent<MissionSound>().sound.isPlaying)
            {

               
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    loadsceneTime = 10;
                


                
            }
        }
    }

    void FindAllEnemiesIncludeInactive()
    {
        EnemiesList.Clear();

        if (enemyParent == null)
        {
            Debug.LogWarning("enemyParent är inte tilldelad i Inspector!");
            return;
        }

        Transform[] allChildren = enemyParent.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allChildren)
        {
            if (t.CompareTag("Enemie"))
            {
                EnemiesList.Add(t.gameObject);
            }
        }
    }

    public void AddEnemyToMission(GameObject newEnemy)
    {
        if (newEnemy != null && !EnemiesList.Contains(newEnemy))
        {
            EnemiesList.Add(newEnemy);
            hasProcessedEnemies = true;
        }
    }
}