
using System.Collections.Generic;



[System.Serializable]


public class SaveData
{
    public int PlayerArmor;
    public float health;
    public string SavedMap;
    public int CurrentMap;
    public int LoadGameCount;
    public int SaveSlot;
    public int Bolt_;
    public int Ammo;

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

    public WeaponSaveData weaponSaveData = new WeaponSaveData(); // <-- Lägg till detta!

    public bool IsValid()
    {
        return health >= 0 &&
               !string.IsNullOrEmpty(SavedMap) &&
               CurrentMap >= 0 &&
               SaveSlot >= 0;
    }
}
