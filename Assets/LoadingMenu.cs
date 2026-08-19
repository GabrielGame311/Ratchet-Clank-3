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

    private float startTime;
    private bool isLoadingComplete = false; // Förhindrar att laddningen körs flera gånger

    void Start()
    {
        Instance = this;
        startTime = Time.time;
    }

    void Update()
    {
        // Om laddningen redan är klar, gör ingenting mer
        if (isLoadingComplete) return;

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
            isLoadingComplete = true; // Lås Update
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
            }

            if (!hasSelectedMap)
            {
                LoadMapName.Instance.SpawnNewGame();
            }
        }

        SceneManager.LoadScene("LoadingMap 1");
    }
}