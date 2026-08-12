using UnityEngine;
using System.IO;
using System.Collections.Generic;

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
            SavedMap = LoadingScene.instance != null ? LoadingScene.instance.LoadMap : AllGameData.Instance.SavedMap,
            CurrentMap = LoadingScene.instance != null ? LoadingScene.instance.MapID : AllGameData.Instance.CurrentMapInt,
            LoadGameCount = LoadMapName.Instance?.LoadGames_?.Count ?? 0,
            health = Player.Player_.maxHealth
        };

        // Hämta bolts om komponenten finns
        Bolts boltsObj = Object.FindObjectOfType<Bolts>();
        if (boltsObj != null)
        {
            data.Bolt_ = boltsObj.bolt;
        }

        // Säkerställ att vapendatastrukturen är initierad
        EnsureWeaponSaveDataExists(data);

        // Hämta alla vapen i scenen (även inaktiva)
        WeaponsUI[] allWeaponUI = Object.FindObjectsOfType<WeaponsUI>(true);
        foreach (var weaponUI in allWeaponUI)
        {
            if (weaponUI != null)
            {
                string wName = string.IsNullOrEmpty(weaponUI.weaponName) ? weaponUI.gameObject.name : weaponUI.weaponName;

                data.weaponSaveData.weapons.Add(new SaveData.WeaponData
                {
                    weaponName = wName,
                    weaponLevel = weaponUI.level
                });
            }
        }

        // Spara till fil
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SaveUtility.GetSavePath(saveSlot), json);

        SaveUI saveUI = Object.FindObjectOfType<SaveUI>();
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
            Bolt_ = 0
        };

        EnsureWeaponSaveDataExists(data);

        // Lägg till startvapen från scenen med Level 1
        WeaponsUI[] allWeaponUI = Object.FindObjectsOfType<WeaponsUI>(true);
        foreach (var weaponUI in allWeaponUI)
        {
            if (weaponUI != null)
            {
                string wName = string.IsNullOrEmpty(weaponUI.weaponName) ? weaponUI.gameObject.name : weaponUI.weaponName;

                data.weaponSaveData.weapons.Add(new SaveData.WeaponData
                {
                    weaponName = wName,
                    weaponLevel = 1
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SaveUtility.GetSavePath(saveSlot), json);

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
            if (prefab) ApplyMapData(data, saveSlot);

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

        // Ladda vapendatans levels
        ApplyWeaponsData(data);

        // Ladda Bolts
        Bolts boltsObj = Object.FindObjectOfType<Bolts>();
        if (boltsObj != null)
        {
            boltsObj.bolt = data.Bolt_;
        }

        // Ladda Map/Scene inställningar
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

        if (Player.Player_ != null)
        {
            Player.Player_.maxHealth = (int)data.health;
        }
    }

    private static void ApplyMapData(SaveData data, int saveSlot)
    {
        if (LoadMapName.Instance == null)
        {
            Debug.LogWarning("LoadMapName.Instance is null! Skipping map data application.");
            return;
        }

        LoadMapName.Instance.mapid = data.CurrentMap;
        LoadMapName.Instance.SpawnPrefabs();

        ApplyGameData(data, saveSlot);
    }

    private static void ApplyWeaponsData(SaveData data)
    {
        if (data.weaponSaveData == null || data.weaponSaveData.weapons == null || data.weaponSaveData.weapons.Count == 0)
        {
            Debug.LogWarning("No weapon data found in save file.");
            return;
        }

        // Hämta alla vapen (inklusive inaktiva)
        WeaponsUI[] allWeaponUI = Object.FindObjectsOfType<WeaponsUI>(true);

        foreach (var savedWeapon in data.weaponSaveData.weapons)
        {
            bool weaponFound = false;

            foreach (var weaponUI in allWeaponUI)
            {
                string wName = string.IsNullOrEmpty(weaponUI.weaponName) ? weaponUI.gameObject.name : weaponUI.weaponName;

                if (wName == savedWeapon.weaponName)
                {
                    weaponUI.level = savedWeapon.weaponLevel;
                    Debug.Log($"Loaded weapon {savedWeapon.weaponName} with level {savedWeapon.weaponLevel}");
                    weaponFound = true;
                    break;
                }
            }

            if (!weaponFound)
            {
                Debug.LogWarning($"Weapon {savedWeapon.weaponName} not found in the scene.");
            }
        }
    }

    private static void EnsureWeaponSaveDataExists(SaveData data)
    {
        if (data.weaponSaveData == null)
        {
            data.weaponSaveData = new SaveData.WeaponSaveData();
        }
        if (data.weaponSaveData.weapons == null)
        {
            data.weaponSaveData.weapons = new List<SaveData.WeaponData>();
        }
    }
}