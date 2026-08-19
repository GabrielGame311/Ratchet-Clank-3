using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    public int saveSlot;

    public GameObject ContinuePoint_; // UI-knapp eller panel för Continue

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

        Debug.Log("LoadMapName Start() called in scene: " + gameObject.scene.name);

        // 1. Kontrollera om det finns sparade filer och visa/dölj Continue-knappen
        CheckContinueButton();
        SpawnPrefabs();

        if (LoadingExistingSave && !DirectMapNavigation)
        {
            // Load save data only when the user selected a save card.
            SaveSystem.LoadGame(saveSlot, true);
        }
        else
        {
            // Direct ship navigation must keep its selected map.
            LoadMap = NextMapToLoad;
            mapid = NextMapId;
        }
    }

    /// <summary>
    /// Aktiverar ContinuePoint_ om minst en sparfil (slot 0-4) existerar
    /// </summary>
    public void CheckContinueButton()
    {
        bool hasSave = false;

        for (int i = 0; i < 5; i++)
        {
            if (File.Exists(SaveUtility.GetSavePath(i)))
            {
                hasSave = true;
                break;
            }
        }

        if (ContinuePoint_ != null)
        {
            ContinuePoint_.SetActive(hasSave);
        }
    }

    /// <summary>
    /// Anropa denna via Button OnClick() på din "Continue"-knapp i UI:t
    /// </summary>
    public void OnContinuePressed()
    {
        // Laddar slot 0 (eller den senast sparade slotten)
        if (File.Exists(SaveUtility.GetSavePath(saveSlot)))
        {
            SaveSystem.LoadGame(saveSlot, false);
        }
        else
        {
            // Om vald slot saknas, leta efter första tillgängliga slot
            for (int i = 0; i < 5; i++)
            {
                if (File.Exists(SaveUtility.GetSavePath(i)))
                {
                    saveSlot = i;
                    NextSaveSlot = saveSlot;
                    SaveSystem.LoadGame(saveSlot, false);
                    break;
                }
            }
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

        // Uppdatera synligheten för Continue-knappen
        CheckContinueButton();

        Debug.Log("Spawned and saved new prefab in slot: " + saveSlot);
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