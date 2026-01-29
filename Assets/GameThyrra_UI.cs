using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;





public class GameThyrra_UI : MonoBehaviour
{
    public ButtonSpawnGame currentActiveKey = null;

    public static GameThyrra_UI Instance;
    public Transform ContentSpawnDown;
    public Transform SpawnButtonUppContent;
    public GameObject ButtonSpawn;
    public RectTransform ButtonSpawnZone; // den vita rutan
    public float TimeRooling;

    float StartRooling;
    public List<string> KeycodeRooling;
    public static bool IsOn = true;
    public int KeycodeInt;
    public GameObject GamePanel;
    public static bool isThyrraGame = false;
    public List<string> StartKeyrooling;
    public List<GameObject> Buttons_;
    public GameObject Info_UI;

    void Start()
    {
        
            
        StartKeyrooling.AddRange(KeycodeRooling);
        
        Instance = GetComponent<GameThyrra_UI>();
        StartRooling = TimeRooling;
        isThyrraGame = true;
        
    }

    void Update()
    {

        if (KeycodeRooling.Count == 0)
        {
            isThyrraGame = false;
            StartCoroutine(Wait());
        }
        if (isThyrraGame)
        {
            if (GamePanel.activeSelf)
            {


                if (KeycodeInt < KeycodeRooling.Count)
                {
                    TimeRooling -= Time.deltaTime;
                }
                    
                
            }

            if (TimeRooling < 0)
            {
                TimeRooling = StartRooling;
                GameObject sp = Instantiate(ButtonSpawn, ContentSpawnDown.transform);
                sp.transform.position = ContentSpawnDown.transform.position;

                var buttonScript = sp.GetComponent<ButtonSpawnGame>();
                buttonScript.KeyText = KeycodeRooling[KeycodeInt];
                buttonScript.targetZone = ButtonSpawnZone;
                buttonScript.spawnUppParent = SpawnButtonUppContent;
               
                //buttonScript.Venster = -200;

                Buttons_.Add(sp);

                // Endast sätt första knappen som aktiv
               
                    currentActiveKey = buttonScript;
                
                if (KeycodeInt < KeycodeRooling.Count)
                {
                    //KeycodeInt++;
                    
                }

               
            }
            


        }

        if (!isThyrraGame && Input.GetKeyDown(KeyCode.E))
        {
            ResetGame(); // Endast från ett ställe!
        }

        if (isThyrraGame && currentActiveKey != null && IsOn)
        {
            KeyCode expectedKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), currentActiveKey.KeyText);

            if (Input.GetKeyDown(expectedKey))
            {
                Debug.Log($"[KEY PRESS] Tryckte: {expectedKey}, isInZone: {currentActiveKey.isInZone}, this: {currentActiveKey.name}");

                currentActiveKey.isHandled = true;
                SpawnResultCopy(currentActiveKey, currentActiveKey.isInZone);
               

                if (!currentActiveKey.isInZone)
                {
                    
                    isThyrraGame = false;
                    IsOn = false;
                }
                else
                {
                    KeycodeRooling.RemoveAt(0);
                }

                currentActiveKey = null;
                SetNextActiveKey();
            }
        }


    }



    public void SetNextActiveKey()
    {
        foreach (var button in Buttons_)
        {
            var btn = button.GetComponent<ButtonSpawnGame>();
            if (!btn.isHandled)
            {
                currentActiveKey = btn;
                Debug.Log($"[NEXT ACTIVE] currentActiveKey set to: {btn.name}, KeyText: {btn.KeyText}");
                return;
            }
        }

        
    
    }




    public void SpawnResultCopy(ButtonSpawnGame prefab, bool isCorrect)
    {
        GameObject copy = Instantiate(prefab.gameObject, prefab.spawnUppParent.transform);
        copy.transform.position = prefab.spawnUppParent.transform.position;
        copy.GetComponent<ButtonSpawnGame>().Venster = -200;
        if (!isCorrect)
        {
            copy.GetComponent<Image>().color = Color.red;
        }
        else
        {
            copy.GetComponent<Image>().color = Color.white;
           
           
        }
    }




    public void ResetGame()
    {
        KeycodeRooling.Clear();
        KeycodeRooling.AddRange(StartKeyrooling);
        KeycodeInt = 0;
        isThyrraGame = true;
        IsOn = true;
        Buttons_.Clear();
        // Rensa alla knappar i rullningen
        foreach (Transform child in ContentSpawnDown)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in SpawnButtonUppContent)
        {
            Destroy(child.gameObject);
        }


        // (valfritt) visa en "Tryck E för att börja om"-UI
    }


    public void ResetObj()
    {
        // Återställ index
        KeycodeInt = 0;

        // Rensa alla rullande knappar
        foreach (Transform child in ContentSpawnDown)
        {
            Destroy(child.gameObject);
        }

        // (valfritt) visa en "Tryck E för att börja om"-UI
    }
    IEnumerator Wait()
    {


        yield return new WaitForSeconds(3);
        
        GameObject.FindObjectOfType<ThyrraGame>().EndDialog();
        GamePanel.SetActive(false);
        enabled = false; // stoppar Update()
        Debug.Log("Alla tangenter har rullats klart.");


    }


    public void IsSpawn()
    {



    }

}
