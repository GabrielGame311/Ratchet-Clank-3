using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingMenu : MonoBehaviour
{
    public TMP_Text LoadProcentText;
    public Slider SliderLoading;
    public float LoadingTime = 5.0f;

    public static LoadingMenu Instance;
    public string LoadScene;
    public int StartMap;
    public Image image_;

    private float startTime;
    private bool isLoadingActive = false; // Håller koll på om mätaren ska snurra

    private void Awake()
    {
        Instance = this;
       
        
            // Skicka bilden (Sprite) om den finns
            Sprite currentSprite = (image_ != null) ? image_.sprite : null;
            
            // Starta laddningssekvensen
            StartLoading(LoadScene, currentSprite);
        
    }

    /// <summary>
    /// Anropas från LoadGame när en sparlåda klickas på
    /// </summary>
    public void StartLoading(string sceneToLoad, Sprite mapSprite)
    {
        LoadScene = sceneToLoad;
        
        // Sätt bilden på UI:t om den skickades med
        if (image_ != null && mapSprite != null)
        {
            image_.sprite = mapSprite;
        }

        // Återställ timern
        startTime = Time.time;
        isLoadingActive = true;

        // Tänd laddningspanelen om den är släckt
        gameObject.SetActive(true);
    }

    private void Update()
    {
        // Kör endast om laddningen faktiskt har startats via StartLoading()
        if (!isLoadingActive) return;

        // Beräkna framsteg (0 till 1)
        float progress = (Time.time - startTime) / LoadingTime;
        progress = Mathf.Clamp01(progress);

        // Uppdatera UI
        if (SliderLoading != null)
        {
            SliderLoading.value = progress;
        }

        if (LoadProcentText != null)
        {
            LoadProcentText.text = Mathf.Round(progress * 100f) + "%";
        }

        // När timern når 100%
        if (progress >= 1.0f)
        {
            isLoadingActive = false; // Lås Update
            CompleteLoading();
        }
    }

    private void CompleteLoading()
    {
        Debug.Log("Loading complete!");

        bool loadingExistingSave = LoadMapName.LoadingExistingSave;
        bool directMapNavigation = LoadMapName.DirectMapNavigation;
        bool hasSelectedMap = (loadingExistingSave || directMapNavigation) && !string.IsNullOrEmpty(LoadMapName.NextMapToLoad);

        if (!hasSelectedMap)
        {
            LoadMapName.NextMapToLoad = LoadScene;
            LoadMapName.LoadingExistingSave = false;
            LoadMapName.DirectMapNavigation = false;
            LoadingScene.HasPendingDirectMap = false;
        }

        LoadMapName.NextSaveSlot = LoadMapName.Instance != null ? LoadMapName.Instance.saveSlot : LoadMapName.NextSaveSlot;

        if (LoadMapName.Instance != null)
        {
            if (!hasSelectedMap)
            {
                LoadMapName.Instance.LoadMap = LoadScene;
                LoadMapName.Instance.mapid = StartMap;
                LoadMapName.Instance.SpawnNewGame();
            }
        }

        SceneManager.LoadScene("LoadingMap 1");
    }
}