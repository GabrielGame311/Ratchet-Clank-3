using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;
using System.Collections;


public class LoadGame : MonoBehaviour
{
    public Image image_;
    public string CurrentMap;
    public int saveSlot;
    public Button Button_;

    public static LoadGame instance;

    void Start()
    {
        instance = this;
        LoadSavedData();
       
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LoadingMap 1")
        {
            SaveSystem.LoadGame(saveSlot, false);
        }
    }

    private void LoadSavedData()
    {
        string path = SaveUtility.GetSavePath(saveSlot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save not found in slot " + saveSlot);
            return;
        }
        
        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        CurrentMap = data.SavedMap;
        
        //saveSlot = data.SaveSlot;
        Debug.Log("Loaded slot " + saveSlot + ": " + CurrentMap + ", index: " + data.CurrentMap);

        if (image_ != null && AllGameData.Instance != null && AllGameData.Instance.ImageMap != null)
        {
            if (data.CurrentMap >= 0 && data.CurrentMap < AllGameData.Instance.ImageMap.Length)
            {
                image_.sprite = AllGameData.Instance.ImageMap[data.CurrentMap];
            }
            else
            {
                Debug.LogWarning("Invalid CurrentMap index: " + data.CurrentMap);
            }
        }
    }

    public void SetSaveSlot(int slot)
    {
        
        saveSlot = slot;
       
        LoadSavedData();
        
        Debug.Log("Selected slot: " + saveSlot);
    }



    public void LoadGameScene()
    {
        string path = SaveUtility.GetSavePath(saveSlot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Save not found in slot {saveSlot}");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        AllGameData.Instance.CurrentMapInt = data.CurrentMap;
        AllGameData.Instance.CurrentSaveSlot = saveSlot;
        LoadMapName.Instance.LoadMap = data.SavedMap;
        LoadMapName.Instance.saveSlot = saveSlot;

        if (SceneUtility.GetBuildIndexByScenePath("LoadingMap 1") == -1)
        {
            Debug.LogError("Scene 'LoadingMap 1' not found in build settings!");
            return;
        }
       
        if(LoadingScene.instance != null)
        {
            LoadingScene.instance.SaveSlot = saveSlot;
        }

        SceneManager.LoadScene("LoadingMap 1");
       
    }


}