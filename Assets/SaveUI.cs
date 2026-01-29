using UnityEngine;
using TMPro;

public class SaveUI : MonoBehaviour
{
    public GameObject savingPanel; // Reference to your "Saving..." UI
    public float SaveTime = 6;
    public void ShowSavingMessage()
    {

        if(AllGameData.Instance != null)
        {
            SaveSystem.LoadGame(AllGameData.Instance.CurrentSaveSlot, false);
        }

        savingPanel.SetActive(true);
        CancelInvoke(nameof(HideSavingMessage)); // Prevent overlap
        Invoke(nameof(HideSavingMessage), SaveTime);   // Show for 2 seconds
    }

    private void HideSavingMessage()
    {
        savingPanel.SetActive(false);
    }
}
