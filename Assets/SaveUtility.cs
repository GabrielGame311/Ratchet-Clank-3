using UnityEngine;

public static class SaveUtility
{
    public static string GetSavePath(int slot)
    {
        return Application.persistentDataPath + $"/save_{slot}.json";
    }
}