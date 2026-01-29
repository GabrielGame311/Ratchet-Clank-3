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

        string json = JsonUtility.ToJson(gameData);

        string encrypted = EncryptionDecryption(json);

        using (StreamWriter writer = new StreamWriter(jsonPathProject))
        {

            writer.Write(encrypted);
            print("Saved Game to Json file at :" + jsonPathProject + slotNumber + ".json");

        };


        








    }

    public AllGameData LoadGameDataFromJsonFile(int slotNumber)
    {
        using(StreamReader reader = new StreamReader (jsonPathProject))
        {

            string json = reader.ReadToEnd();

            string decrypted = EncryptionDecryption(json);

            AllGameData data = JsonUtility.FromJson<AllGameData>(decrypted);
            return data;

        };


    }

}
