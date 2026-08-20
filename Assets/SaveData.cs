using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int PlayerArmor;
    public float health;
    public string SavedMap;
    public int CurrentMap;
    public int ImageMapIndex;
    public int LoadGameCount;
    public int SaveSlot;
    public int Bolt_;
    public int Ammo;
    public string timePlayed;
    public int bolts;
    public int challengeMode;
    public string saveDate;

    public int currentSaveSlot;
    public int currentMapInt;
    public int armor;

    // Position och rotation
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public bool hasCheckpoint;

    [System.Serializable]
    public class WeaponData
    {
        public string weaponName;
        public int weaponLevel;
    }

    [System.Serializable]
    public class WeaponSaveData
    {
        public List<WeaponData> weapons = new List<WeaponData>();
    }

    public WeaponSaveData weaponSaveData = new WeaponSaveData();

    public bool IsValid()
    {
        return health >= 0 &&
               !string.IsNullOrEmpty(SavedMap) &&
               CurrentMap >= 0 &&
               SaveSlot >= 0;
    }
}