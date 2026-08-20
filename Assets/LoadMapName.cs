using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class LoadMapName : MonoBehaviour
{
    public string LoadMap;
    public static string NextMapToLoad = "";
    public static int NextSaveSlot;
    public static int NextMapId;
    public static bool LoadingExistingSave;
    public static bool DirectMapNavigation;
    public int mapid;
    public static LoadMapName Instance;

    public Transform Content_;
    public GameObject LoadGamePrefab;
    public List<GameObject> LoadGames_ = new List<GameObject>();
    public GameObject LoadWait_;
    public int saveSlot;

    public GameObject ContinuePoint_; // UI-knapp/panel för "Continue" i huvudmenyn

    public static void SetDirectMap(string sceneName, int mapId)
    {
        LoadingExistingSave = false;
        DirectMapNavigation = true;
        NextMapToLoad = sceneName;
        NextMapId = mapId;
        LoadingScene.PendingLoadMap = sceneName;
        LoadingScene.PendingMapId = mapId;
        LoadingScene.HasPendingDirectMap = true;
    }

    void Start()
    {
        Time.timeScale = 1f;
        Menu.IsMenu = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Instance = this;
        saveSlot = NextSaveSlot;

        // Kontrollera sparfiler och bygg menyn
        CheckContinueButton();
        SpawnPrefabs();

        if (LoadingExistingSave && !DirectMapNavigation)
        {
            SaveSystem.LoadGame(saveSlot, true);
        }
        else
        {
            LoadMap = NextMapToLoad;
            mapid = NextMapId;
        }
    }

    /// <summary>
    /// Aktiverar Continue-knappen om minst en sparfil finns
    /// </summary>
    public void CheckContinueButton()
    {
        // Dölj UI-objektet när scenen startar så det bara visas vid trigger
        if (ContinuePoint_ != null)
        {
            ContinuePoint_.SetActive(false);
        }
    }

    /// <summary>
    /// KOPPLA DENNA TILL DIN "CONTINUE"-KNAPP I HUVUDMENYN (On Click)
    /// </summary>
    public void OnContinuePressed()
    {
        int slotToLoad = saveSlot;

        // Om vald slot saknas, leta reda på den senaste/första giltiga sparfilen
        if (!File.Exists(SaveUtility.GetSavePath(slotToLoad)))
        {
            slotToLoad = -1;
            for (int i = 0; i < 5; i++)
            {
                if (File.Exists(SaveUtility.GetSavePath(i)))
                {
                    slotToLoad = i;
                    break;
                }
            }
        }

        // Om en giltig sparfil hittades:
      

        if (slotToLoad != -1)
        {
            // REA BORT temporära checkpoints inför ny laddning
            TempCheckpoint.Reset();

            if (AllGameData.Instance != null)
            {
                AllGameData.Instance.hasCheckpoint = false;
            }

            SaveSystem.LoadGame(slotToLoad, false);
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingMap 1");
        }
    }

    public void SpawnNewGame()
    {
        if (LoadGamePrefab == null || Content_ == null)
        {
            Debug.LogError("Cannot create a save slot: LoadGamePrefab or Content_ is not assigned.");
            return;
        }

        int availableSlot = FindAvailableSlot();
        if (availableSlot < 0)
        {
            Debug.Log("Maximum of 5 save games reached.");
            return;
        }

        saveSlot = availableSlot;
        NextSaveSlot = saveSlot;
        GameObject prefab = Instantiate(LoadGamePrefab, Content_);
        prefab.SetActive(false);
        LoadGame loadGame = prefab.GetComponent<LoadGame>();
        if (loadGame == null)
        {
            Debug.LogError("LoadGamePrefab is missing a LoadGame component.");
            Destroy(prefab);
            return;
        }

        loadGame.saveSlot = saveSlot;
        LoadGames_.Add(prefab);
        SaveSystem.SaveNewGame(saveSlot);
        prefab.SetActive(true);

        CheckContinueButton();
    }

    private int FindAvailableSlot()
    {
        for (int slot = 0; slot < 5; slot++)
        {
            if (!File.Exists(SaveUtility.GetSavePath(slot)))
            {
                return slot;
            }
        }

        return -1;
    }

    public void SpawnPrefabs()
    {
        if (LoadGamePrefab == null || Content_ == null)
        {
            Debug.LogError("Cannot display save slots: LoadGamePrefab or Content_ is not assigned.");
            return;
        }

        foreach (GameObject prefab in LoadGames_)
        {
            Destroy(prefab);
        }
        LoadGames_.Clear();

        for (int i = 0; i < 5; i++)
        {
            if (File.Exists(SaveUtility.GetSavePath(i)))
            {
                GameObject prefab = Instantiate(LoadGamePrefab, Content_);
                prefab.SetActive(false);
                LoadGame loadGame = prefab.GetComponent<LoadGame>();
                if (loadGame != null)
                {
                    loadGame.saveSlot = i;
                    LoadGames_.Add(prefab);
                    prefab.SetActive(true);
                }
                else
                {
                    Debug.LogError("LoadGamePrefab is missing a LoadGame component.");
                    Destroy(prefab);
                }
            }
        }

        CheckContinueButton();
    }
}