using UnityEngine;
using System.IO;

public static class SaveUtility
{
    public static string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.json");
    }

    public static bool TryReadSave(int slot, out SaveData data)
    {
        data = null;
        string path = GetSavePath(slot);

        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning($"Save file in slot {slot} is empty.");
                return false;
            }

            data = JsonUtility.FromJson<SaveData>(json);
            return data != null;
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Could not read save slot {slot}: {exception.Message}");
            return false;
        }
    }

    public static bool TryWriteSave(int slot, SaveData data)
    {
        if (slot < 0 || data == null)
        {
            Debug.LogError($"Could not save slot {slot}: invalid save data.");
            return false;
        }

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string path = GetSavePath(slot);
            string temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));

            if (File.Exists(path))
            {
                File.Replace(temporaryPath, path, null);
            }
            else
            {
                File.Move(temporaryPath, path);
            }

            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Could not write save slot {slot}: {exception.Message}");
            return false;
        }
    }
}