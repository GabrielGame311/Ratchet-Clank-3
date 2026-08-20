using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveSystem
{
    // Överlagrad metod för sparande när man väljer bana i ShipMenu
    public static void SaveGame(int saveSlot, string overrideMap, int overrideMapId)
    {
        SaveGameInternal(saveSlot, overrideMap, overrideMapId);
    }

    // Standard spar-metod
    public static void SaveGame(int saveSlot)
    {
        SaveGameInternal(saveSlot, null, -1);
    }

    private static void SaveGameInternal(int saveSlot, string overrideMap, int overrideMapId)
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

        // Bestäm vilken bana som ska sparas
        string targetMap = !string.IsNullOrEmpty(overrideMap) ? overrideMap : 
                           (LoadingScene.instance != null ? LoadingScene.instance.LoadMap : AllGameData.Instance.SavedMap);

        int targetMapId = overrideMapId >= 0 ? overrideMapId : 
                          (LoadingScene.instance != null ? LoadingScene.instance.MapID : AllGameData.Instance.CurrentMapInt);

        // Nollställ alltid checkpoint vid filsparning så att omstart från menyn alltid laddar vid banans start
        SaveData data = new SaveData
        {
            SaveSlot = saveSlot,
            currentSaveSlot = saveSlot,
            PlayerArmor = armor,
            armor = armor,
            SavedMap = targetMap,
            CurrentMap = targetMapId,
            currentMapInt = targetMapId,
            LoadGameCount = LoadMapName.Instance?.LoadGames_?.Count ?? 0,
            health = health,
            saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            
            // Tvinga checkpoints att ALDRIG sparas permanent till hårddisken
            posX = 0f,
            posY = 0f,
            posZ = 0f,
            rotX = 0f,
            rotY = 0f,
            rotZ = 0f,
            rotW = 1f,
            hasCheckpoint = false
        };

        data.ImageMapIndex = GetImageMapIndex(data.SavedMap, data.CurrentMap);

        // Hämta bolts om komponenten finns och synka båda fälten
        int boltsAmount = 0;
        Bolts boltsObj = Object.FindObjectOfType<Bolts>();
        if (boltsObj != null && Player.Player_ != null)
        {
            boltsAmount = boltsObj.bolt;
        }
        else if (SaveUtility.TryReadSave(saveSlot, out SaveData previousSave))
        {
            boltsAmount = previousSave.Bolt_;
        }
        data.Bolt_ = boltsAmount;
        data.bolts = boltsAmount;

        // Säkerställ att vapendatastrukturen är initierad och rensad från gamla poster
        EnsureWeaponSaveDataExists(data);
        data.weaponSaveData.weapons.Clear(); // Rensar listan för att förhindra dubbleringar i JSON

        HashSet<string> addedWeapons = new HashSet<string>();

        // Hämta alla unik vapen i scenen (även inaktiva)
        WeaponsUI[] allWeaponUI = Object.FindObjectsOfType<WeaponsUI>(true);
        foreach (var weaponUI in allWeaponUI)
        {
            if (weaponUI != null)
            {
                string wName = string.IsNullOrEmpty(weaponUI.weaponName) ? weaponUI.gameObject.name : weaponUI.weaponName;

                // Undvik att spara samma vapennamn flera gånger
                if (!addedWeapons.Contains(wName))
                {
                    addedWeapons.Add(wName);
                    data.weaponSaveData.weapons.Add(new SaveData.WeaponData
                    {
                        weaponName = wName,
                        weaponLevel = weaponUI.level
                    });
                }
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

        Debug.Log($"Game saved to slot {saveSlot} | Map: {data.SavedMap} (ID: {data.CurrentMap})");
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
            armor = 0,
            health = 10,
            SavedMap = newMap,
            CurrentMap = newMapId,
            currentMapInt = newMapId,
            LoadGameCount = 0,
            SaveSlot = saveSlot,
            currentSaveSlot = saveSlot,
            Bolt_ = 0,
            bolts = 0,
            saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            hasCheckpoint = false
        };
        data.ImageMapIndex = GetImageMapIndex(data.SavedMap, data.CurrentMap);

        EnsureWeaponSaveDataExists(data);
        data.weaponSaveData.weapons.Clear();

        HashSet<string> addedWeapons = new HashSet<string>();

        WeaponsUI[] allWeaponUI = Object.FindObjectsOfType<WeaponsUI>(true);
        foreach (var weaponUI in allWeaponUI)
        {
            if (weaponUI != null)
            {
                string wName = string.IsNullOrEmpty(weaponUI.weaponName) ? weaponUI.gameObject.name : weaponUI.weaponName;

                if (!addedWeapons.Contains(wName))
                {
                    addedWeapons.Add(wName);
                    data.weaponSaveData.weapons.Add(new SaveData.WeaponData
                    {
                        weaponName = wName,
                        weaponLevel = 1
                    });
                }
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

        ApplyWeaponsData(data);

        Bolts boltsObj = Object.FindObjectOfType<Bolts>();
        if (boltsObj != null)
        {
            boltsObj.bolt = data.Bolt_;
        }

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

        // Nollställ checkpoint i minnet vid laddning från sparfil/huvudmeny
        AllGameData.Instance.hasCheckpoint = false;
        AllGameData.Instance.lastCheckpointPos = Vector3.zero;
        AllGameData.Instance.lastCheckpointRot = Quaternion.identity;

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

        WeaponsUI[] allWeaponUI = Object.FindObjectsOfType<WeaponsUI>(true);

        foreach (var savedWeapon in data.weaponSaveData.weapons)
        {
            bool weaponFound = false;

            foreach (var weaponUI in allWeaponUI)
            {
                if (weaponUI == null) continue;

                string wName = string.IsNullOrEmpty(weaponUI.weaponName) ? weaponUI.gameObject.name : weaponUI.weaponName;

                if (wName == savedWeapon.weaponName)
                {
                    weaponUI.level = savedWeapon.weaponLevel;
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