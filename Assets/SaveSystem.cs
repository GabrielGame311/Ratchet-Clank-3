using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystem
{
    public static void SaveGame(int saveSlot)
    {
        if (saveSlot < 0)
        {
            Debug.LogError($"Cannot save invalid slot {saveSlot}.");
            return;
        }

        if (AllGameData.Instance == null)
        {
            Debug.LogError("Cannot save: AllGameData.Instance is null.");
            return;
        }

        int health = Player.Player_ != null ? Player.Player_.maxHealth : 10;
        int armor = AllGameData.Instance.Armor;
        if (Player.Player_ == null && SaveUtility.TryReadSave(saveSlot, out SaveData previousData))
        {
            armor = previousData.PlayerArmor;
            health = (int)previousData.health;
        }

        SaveData data = new SaveData
        {
            SaveSlot = saveSlot,
            PlayerArmor = armor,
            SavedMap = LoadingScene.instance != null ? LoadingScene.instance.LoadMap : AllGameData.Instance.SavedMap,
            CurrentMap = LoadingScene.instance != null ? LoadingScene.instance.MapID : AllGameData.Instance.CurrentMapInt,
            LoadGameCount = LoadMapName.Instance?.LoadGames_?.Count ?? 0,
            health = health,
            saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            posX = AllGameData.Instance.lastCheckpointPos.x,
            posY = AllGameData.Instance.lastCheckpointPos.y,
            posZ = AllGameData.Instance.lastCheckpointPos.z,
            rotX = AllGameData.Instance.lastCheckpointRot.x,
            rotY = AllGameData.Instance.lastCheckpointRot.y,
            rotZ = AllGameData.Instance.lastCheckpointRot.z,
            rotW = AllGameData.Instance.lastCheckpointRot.w,
            hasCheckpoint = AllGameData.Instance.hasCheckpoint
        };
        data.ImageMapIndex = GetImageMapIndex(data.SavedMap, data.CurrentMap);

        // Hämta bolts om komponenten finns
        Bolts boltsObj = Object.FindObjectOfType<Bolts>();
        if (boltsObj != null && Player.Player_ != null)
        {
            data.Bolt_ = boltsObj.bolt;
        }
        else if (SaveUtility.TryReadSave(saveSlot, out SaveData previousSave))
        {
            data.Bolt_ = previousSave.Bolt_;
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
        if (!SaveUtility.TryWriteSave(saveSlot, data))
        {
            return;
        }

        SaveUI saveUI = Object.FindObjectOfType<SaveUI>();
        if (saveUI != null)
        {
            saveUI.ShowSavingMessage();
        }

        Debug.Log($"Game saved with {data.LoadGameCount} prefabs in slot {saveSlot}");
    }

    public static void SaveNewGame(int saveSlot)
    {
        string newMap = "Veldins";
        int newMapId = 0;
        if (LoadMapName.Instance != null)
        {
            if (!string.IsNullOrEmpty(LoadMapName.Instance.LoadMap))
            {
                newMap = LoadMapName.Instance.LoadMap;
            }

            newMapId = LoadMapName.Instance.mapid;
        }
        else if (!string.IsNullOrEmpty(LoadMapName.NextMapToLoad))
        {
            newMap = LoadMapName.NextMapToLoad;
        }

        SaveData data = new SaveData
        {
            PlayerArmor = 0,
            health = 10,
            SavedMap = newMap,
            CurrentMap = newMapId,
            LoadGameCount = 0,
            SaveSlot = saveSlot,
            Bolt_ = 0,
            saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        data.ImageMapIndex = GetImageMapIndex(data.SavedMap, data.CurrentMap);

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

        if (!SaveUtility.TryWriteSave(saveSlot, data))
        {
            return;
        }

        Debug.Log($"New game saved to slot {saveSlot}.");
    }

    public static void LoadGame(int saveSlot, bool prefab)
    {
        if (!SaveUtility.TryReadSave(saveSlot, out SaveData data))
        {
            Debug.LogWarning($"Save file could not be loaded from slot {saveSlot}.");
            return;
        }

        try
        {
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
            LoadingScene.instance.MapID = data.CurrentMap;
            LoadingScene.instance.SaveSlot = data.SaveSlot;
        }

        AllGameData.Instance.SavedMap = data.SavedMap;
        AllGameData.Instance.CurrentSaveSlot = data.SaveSlot;

        if (LoadMapName.Instance != null)
        {
            LoadMapName.Instance.LoadMap = data.SavedMap;
            LoadMapName.Instance.mapid = data.CurrentMap;
            LoadMapName.NextMapToLoad = data.SavedMap;
        }

        AllGameData.Instance.CurrentMapInt = data.CurrentMap;
        AllGameData.Instance.SetArmor(data.PlayerArmor);
        AllGameData.Instance.hasCheckpoint = data.hasCheckpoint;
        AllGameData.Instance.lastCheckpointPos = new Vector3(data.posX, data.posY, data.posZ);
        AllGameData.Instance.lastCheckpointRot = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);

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
                if (weaponUI == null)
                {
                    continue;
                }

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

    private static int GetImageMapIndex(string mapName, int mapIndex)
    {
        if (string.Equals(mapName, "Phinix", System.StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        return mapIndex;
    }
}