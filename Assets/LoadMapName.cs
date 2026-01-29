using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LoadMapName : MonoBehaviour
{
    public string LoadMap;
    public int mapid;
    public static LoadMapName Instance;

    public Transform Content_;
    public GameObject LoadGamePrefab;
    public List<GameObject> LoadGames_ = new List<GameObject>();

    public int saveSlot;

   

    void Start()
    {

        Instance = this;

        Debug.Log("LoadMapName Start() called in scene: " + gameObject.scene.name);
        SaveSystem.LoadGame(saveSlot, true); // Load initial save slot

        

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
          //  SpawnNewGame();
        }
    }

    public void SpawnNewGame()
    {
        if (LoadGames_.Count >= 5)
        {
            Debug.Log("Max prefab limit (5) reached!");
            return;
        }

        saveSlot = LoadGames_.Count;
        GameObject prefab = Instantiate(LoadGamePrefab, Content_);
        prefab.GetComponent<LoadGame>().saveSlot = saveSlot;
        //prefab.GetComponent<LoadGame>().image_.sprite = AllGameData.Instance.ImageMap[AllGameData.Instance.CurrentMapInt];
        LoadGames_.Add(prefab);
        SaveSystem.SaveNewGame(saveSlot);
        Debug.Log("Spawned and saved new prefab in slot: " + saveSlot);
    }

    public void SpawnPrefabs()
    {
        foreach (GameObject prefab in LoadGames_)
        {
            Destroy(prefab); // Clean up existing prefabs safely
        }
        LoadGames_.Clear();

        for (int i = 0; i < 5; i++)
        {
            if (File.Exists(SaveUtility.GetSavePath(i)))
            {
                GameObject prefab = Instantiate(LoadGamePrefab, Content_);
                prefab.GetComponent<LoadGame>().saveSlot = i;
                LoadGames_.Add(prefab);
            }
        }
    }
}