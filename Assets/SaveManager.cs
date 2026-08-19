using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;


public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; set; }


    string jsonPathProject;
    string jsonPathPersistent;
   

    public bool IsSavingToJson;


    public Button ButtonNewGame;





    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {


        jsonPathProject = Application.dataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
        jsonPathPersistent = Application.persistentDataPath + Path.AltDirectorySeparatorChar + "SaveGame.json";
    }

    // Update is called once per frame
    void Update()
    {
        
        


    }

    public void LoadGame()
    {
       // SetPlayerData();
    }

    public AllGameData LoadAllGameData()
    {

       AllGameData gameData = new AllGameData();
        return gameData;

    }

    void SetPlayerData(PlayerData playerData)
    {

      //  throw new 
    }
  

    public string EncryptionDecryption(string jsonString)
    {
        string keyword = "1234567";
        string result = "";

        for(int i = 0; i < jsonString.Length; i++)
        {
            result += (char)(jsonString[i] ^ keyword[i % keyword.Length]);
        }

        return result;
    }



    public void SavingTypeSwitch(AllGameData gameData, int slotNumber)
    {
        if(IsSavingToJson)
        {
            SaveGameDataToJsonFile(gameData, slotNumber);
        }
    }


    public void SaveGameDataToJsonFile(AllGameData gameData, int slotNumber)
    {
        if (gameData == null || slotNumber < 0)
        {
            Debug.LogError("Cannot save legacy game data: invalid data or slot.");
            return;
        }

        try
        {
            string path = GetLegacySavePath(slotNumber);
            Directory.CreateDirectory(Application.persistentDataPath);
            string json = JsonUtility.ToJson(gameData);
            string encrypted = EncryptionDecryption(json);
            File.WriteAllText(path, encrypted);

            Debug.Log("Saved legacy game data to: " + path);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Could not save legacy game data: " + exception.Message);
        }

    }

    public AllGameData LoadGameDataFromJsonFile(int slotNumber)
    {
        string path = GetLegacySavePath(slotNumber);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Legacy save not found in slot " + slotNumber);
            return null;
        }

        try
        {
            string encrypted = File.ReadAllText(path);
            string decrypted = EncryptionDecryption(encrypted);
            return JsonUtility.FromJson<AllGameData>(decrypted);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Could not load legacy game data: " + exception.Message);
            return null;
        }
    }

    private string GetLegacySavePath(int slotNumber)
    {
        return Path.Combine(Application.persistentDataPath, "SaveGame_" + slotNumber + ".json");

    }

}
