using UnityEngine;
using System.IO;
using System.Linq;

public static class SaveSystem
{
    public static void SaveGame(int saveSlot)
    {
        if (AllGameData.Instance == null || Player.Player_ == null)
        {
            Debug.LogError("Cannot save: AllGameData.Instance or Player.Player_ is null!");
            return;
        }



        SaveData data = new SaveData
        {
            SaveSlot = saveSlot,

            PlayerArmor = AllGameData.Instance.Armor,
            SavedMap = LoadingScene.instance.LoadMap,
            CurrentMap = LoadingScene.instance.MapID,
            LoadGameCount = LoadMapName.Instance?.LoadGames_.Count ?? 0,
            Bolt_ = GameObject.FindObjectOfType<Bolts>().bolt,
          //  WeaponLvl = GameObject.FindObjectOfType<WeaponsUI>().Level,
            health = Player.Player_.maxHealth
        };


        // Lägg till vapeninfo korrekt:
        WeaponsUI[] allWeaponUI = GameObject.FindObjectsOfType<WeaponsUI>(); // Hitta alla vapenobjekt med WeaponsUI
        foreach (var weaponUI in allWeaponUI)
        {
            // Vi antar att varje WeaponsUI hanterar ett vapen med dess level
            if (weaponUI != null)
            {
                data.weaponSaveData.weapons.Add(new SaveData.WeaponData
                {
                    weaponName = weaponUI.WeaponName,  // Exempel på hur du får vapennamn
                    weaponLevel = weaponUI.Level      // Exempel på hur du får vapennivå
                });
            }
        }

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(SaveUtility.GetSavePath(saveSlot), json);
        SaveUI saveUI = GameObject.FindObjectOfType<SaveUI>();
        if (saveUI != null)
        {
            saveUI.ShowSavingMessage();
        }
        Debug.Log($"Game saved with {data.LoadGameCount} prefabs in slot {saveSlot}");
    }



    public static void SaveNewGame(int saveSlot)
    {
        SaveData data = new SaveData
        {
            PlayerArmor = 0,
            health = 10,
            SavedMap = "Veldins",
            CurrentMap = 0,
            LoadGameCount = 0,
            SaveSlot = saveSlot,
            Bolt_ = 0,
            
            
        };


        // Lägg till ett nytt vapen med level 1
        data.weaponSaveData.weapons.Add(new SaveData.WeaponData
        {
           
            weaponLevel = 1
        });

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(SaveUtility.GetSavePath(saveSlot), json);
       // GameObject.FindObjectOfType<SaveUI>()?.ShowSavingMessage();
        Debug.Log($"New game saved to slot {saveSlot}.");
    }

    public static void LoadGame(int saveSlot, bool prefab)
    {
        string path = SaveUtility.GetSavePath(saveSlot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Save file not found in slot {saveSlot} at path: {path}");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogError($"Failed to deserialize save data from slot {saveSlot}");
                return;
            }

            ApplyGameData(data, saveSlot);
            if (prefab) ApplyMapData(data, saveSlot); // Only spawn prefabs if explicitly requested

            Debug.Log($"Loaded game from slot {saveSlot}: map = {data.SavedMap}, armor = {data.PlayerArmor}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading save file from slot {saveSlot}: {e.Message}");
        }
    }

    private static void ApplyGameData(SaveData data, int saveSlot)
    {
        if (AllGameData.Instance == null)
        {
            Debug.LogError("AllGameData.Instance is null! Cannot apply save data.");
            return;
        }

        if (Player.Player_ == null)
        {
            Debug.LogError("Player.Player_ is null! Cannot set maxHealth.");
            return;
        }



        WeaponsUI[] allWeaponUI = GameObject.FindObjectsOfType<WeaponsUI>();

        if (data.weaponSaveData != null && data.weaponSaveData.weapons.Count > 0)
        {
            foreach (var savedWeapon in data.weaponSaveData.weapons)
            {
                bool weaponFound = false;

                // Loopar genom alla WeaponsUI för att hitta rätt vapen
                foreach (var weaponUI in allWeaponUI)
                {
                    if (weaponUI.WeaponName == savedWeapon.weaponName)
                    {
                        weaponUI.Level = savedWeapon.weaponLevel;  // Uppdatera vapennivån
                        Debug.Log($"Loaded weapon {savedWeapon.weaponName} with level {savedWeapon.weaponLevel}");
                        weaponFound = true;
                        break;  // Stanna när vi hittar rätt vapen
                    }
                }

                if (!weaponFound)
                {
                    Debug.LogWarning($"Weapon {savedWeapon.weaponName} not found in the scene.");
                }
            }
        }
        else
        {
            Debug.LogWarning("No weapon data found in save file.");
        }



        if (GameObject.FindObjectOfType<Bolts>() != null)
        {
            GameObject.FindObjectOfType<Bolts>().bolt = data.Bolt_;
        }

        if(LoadingScene.instance != null)
        {
            LoadingScene.instance.LoadMap = data.SavedMap;
            LoadingScene.instance.SaveSlot = data.SaveSlot;
        }
        else
        {
            AllGameData.Instance.SavedMap = data.SavedMap;
            AllGameData.Instance.CurrentSaveSlot = data.SaveSlot;
        }
        AllGameData.Instance.CurrentMapInt = data.CurrentMap;


        // Ensure armor visuals update
        AllGameData.Instance.SetArmor(data.PlayerArmor);
        if (LoadingScene.instance != null)
        {
            LoadingScene.instance.SaveSlot = data.SaveSlot;
        }
        Player.Player_.maxHealth = (int)data.health;

    }

    private static void ApplyMapData(SaveData data, int saveSlot)
    {
        if (LoadMapName.Instance == null)
        {
            Debug.LogWarning("LoadMapName.Instance is null! Skipping map data application.");
            return;
        }

        string path = SaveUtility.GetSavePath(saveSlot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Save file not found in slot {saveSlot} at path: {path}");
            return;
        }
        if (LoadMapName.Instance != null)
        {
            LoadMapName.Instance.SpawnPrefabs();
        }
        if (GameObject.FindObjectOfType<Bolts>() != null)
        {
            GameObject.FindObjectOfType<Bolts>().bolt = data.Bolt_;
        }


        WeaponsUI[] allWeaponUI = GameObject.FindObjectsOfType<WeaponsUI>();

        if (data.weaponSaveData != null && data.weaponSaveData.weapons.Count > 0)
        {
            foreach (var savedWeapon in data.weaponSaveData.weapons)
            {
                bool weaponFound = false;

                // Loopar genom alla WeaponsUI för att hitta rätt vapen
                foreach (var weaponUI in allWeaponUI)
                {
                    if (weaponUI.WeaponName == savedWeapon.weaponName)
                    {
                        weaponUI.Level = savedWeapon.weaponLevel;  // Uppdatera vapennivån
                        Debug.Log($"Loaded weapon {savedWeapon.weaponName} with level {savedWeapon.weaponLevel}");
                        weaponFound = true;
                        break;  // Stanna när vi hittar rätt vapen
                    }
                }

                if (!weaponFound)
                {
                    Debug.LogWarning($"Weapon {savedWeapon.weaponName} not found in the scene.");
                }
            }
        }
        else
        {
            Debug.LogWarning("No weapon data found in save file.");
        }

        if (Player.Player_ == null)
        {
            Player.Player_.maxHealth = (int)data.health;
        }
        AllGameData.Instance.Armor = data.PlayerArmor;

        if (LoadingScene.instance != null)
        {
            LoadingScene.instance.LoadMap = data.SavedMap;
            LoadingScene.instance.SaveSlot = data.SaveSlot;
        }
        else
        {
            AllGameData.Instance.SavedMap = data.SavedMap;
            AllGameData.Instance.CurrentSaveSlot = data.SaveSlot;
        }


        AllGameData.Instance.CurrentMapInt = data.CurrentMap;
        

        AllGameData.Instance.SetArmor(data.PlayerArmor);
        
        LoadMapName.Instance.mapid = data.CurrentMap;




    }
}