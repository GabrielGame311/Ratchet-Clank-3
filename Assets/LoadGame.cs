using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro; // Tillagt för TextMeshPro-stöd

public class LoadGame : MonoBehaviour
{
    [Header("UI References")]
    public Image image_;
    public Button Button_;

    [Header("Details Panel (PS2 Focus Effect)")]
    public GameObject detailsPanel;       // Panelen som stängs av/på via ScrollFocusScaler
    public TMP_Text mapNameText;           // Namn på banan (t.ex. Starship Phoenix)
    public TMP_Text timePlayedText;        // Speltid (t.ex. Time Played: 80:49)
    public TMP_Text boltsText;             // Antal Bolts (t.ex. Bolts: 89556352)
    public TMP_Text challengeModeText;     // Challenge mode counter
    public TMP_Text dateText;              // Datum/storlek längst upp

    [Header("Save Data Info")]
    public string CurrentMap;
    public int saveSlot;

    private void Start()
    {
        if (image_ == null)
        {
            image_ = GetComponentInChildren<Image>(true);
        }

        // 1. Koppla knappen automatiskt om den finns
        if (Button_ != null)
        {
            Button_.onClick.RemoveAllListeners();
            Button_.onClick.AddListener(LoadGameScene);
        }

        // 2. Ladda visualiseringen för denna specifika slot
        StartCoroutine(LoadSavedDataWhenReady());
    }

    private System.Collections.IEnumerator LoadSavedDataWhenReady()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            LoadSavedData();

            if (image_ == null || image_.sprite != null)
            {
                yield break;
            }

            yield return null;
        }
    }

    public void LoadSavedData()
{
    if (!SaveUtility.TryReadSave(saveSlot, out SaveData data))
    {
        Debug.LogWarning("Save file not found for slot " + saveSlot);
        if (detailsPanel != null) detailsPanel.SetActive(false);
        return;
    }

    CurrentMap = data.SavedMap;

    // 1. Uppdatera bild för banan i UI:t
    AllGameData gameData = AllGameData.Instance;
    if (gameData == null)
    {
        gameData = FindFirstObjectByType<AllGameData>(FindObjectsInactive.Include);
    }

    int imageMapIndex = GetImageMapIndex(data);
    if (image_ == null)
    {
        Debug.LogWarning("LoadGame has no Image component assigned or found in its children.");
    }
    else if (gameData == null || gameData.ImageMap == null)
    {
        Debug.LogWarning("AllGameData.ImageMap is not available for save slot " + saveSlot + ".");
    }
    else if (imageMapIndex < 0 || imageMapIndex >= gameData.ImageMap.Length)
    {
        Debug.LogWarning("Map image index " + imageMapIndex + " is outside AllGameData.ImageMap for save slot " + saveSlot + ".");
    }
    else if (gameData.ImageMap[imageMapIndex] == null)
    {
        Debug.LogWarning("AllGameData.ImageMap element " + imageMapIndex + " is empty.");
    }
    else
    {
        image_.sprite = gameData.ImageMap[imageMapIndex];
    }

    // 2. Uppdatera textfälten säkert (visas bara om textobjektet är kopplat i Inspector)
    if (mapNameText != null) 
        mapNameText.text = !string.IsNullOrEmpty(data.SavedMap) ? data.SavedMap : "Unknown Area";

    // Om du vill visa statisk text tills du hinner lägga till variablerna i SaveData:
    if (timePlayedText != null) timePlayedText.text = "Time Played: --:--";
    if (boltsText != null) boltsText.text = "Bolts: " + data.Bolt_;
    if (challengeModeText != null) challengeModeText.text = "";
    if (dateText != null) dateText.text = data.saveDate ?? "";

    Debug.Log($"Slot {saveSlot} loaded: {CurrentMap}");
}

    private int GetImageMapIndex(SaveData data)
    {
        if (string.Equals(data.SavedMap, "Phinix", System.StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (data.ImageMapIndex >= 0)
        {
            return data.ImageMapIndex;
        }

        return data.CurrentMap;
    }

    public void SetSaveSlot(int slot)
    {
        saveSlot = slot;
        LoadSavedData();
    }

    /// <summary>
    /// Anropas när spelaren klickar på denna sparfil i menyn
    /// </summary>
    public void LoadGameScene()
    {
        if (!SaveUtility.TryReadSave(saveSlot, out SaveData data))
        {
            Debug.LogWarning("Cannot load save slot " + saveSlot);
            return;
        }

        // Sätt kartnamnet statiskt från sparfilen
        string selectedMap = string.IsNullOrEmpty(data.SavedMap) ? "Veldins" : data.SavedMap;
        LoadMapName.NextMapToLoad = selectedMap;
        LoadMapName.NextSaveSlot = saveSlot;
        LoadMapName.LoadingExistingSave = true;
        LoadMapName.DirectMapNavigation = false;
        LoadingScene.HasPendingDirectMap = false;

        if (LoadMapName.Instance != null)
        {
            LoadMapName.Instance.saveSlot = saveSlot;
            LoadMapName.Instance.LoadMap = selectedMap;
            LoadMapName.Instance.mapid = data.CurrentMap;
        }

        if (AllGameData.Instance != null)
        {
            AllGameData.Instance.CurrentSaveSlot = saveSlot;
            AllGameData.Instance.CurrentMapInt = data.CurrentMap;
        }

        SceneManager.LoadScene("LoadingMap 1");
    }
}