using System.Collections;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    Transform spawnLocation; // Valfri specifik spawn-punkt, annars används detta objekts position
    
    private bool isActivated = false;


    void Start()
    {
        spawnLocation = GetComponent<Transform>();
    }



    private void OnTriggerEnter(Collider other)
    {
        // Kolla om det är spelaren som kliver i triggern
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            Vector3 spawnPos = (spawnLocation != null) ? spawnLocation.position : transform.position;
            Quaternion spawnRot = (spawnLocation != null) ? spawnLocation.rotation : transform.rotation;

            if (AllGameData.Instance != null)
            {
                // 1. Spara positionen i AllGameData
                AllGameData.Instance.SetCheckpoint(spawnPos, spawnRot);

                // 2. Spara till fil via ditt SaveSystem
                SaveSystem.SaveGame(AllGameData.Instance.CurrentSaveSlot);

                // 3. Visa "Saving..." UI
                StartCoroutine(wait());
                SaveUI saveUI = FindObjectOfType<SaveUI>();
                if (saveUI != null)
                {
                    //saveUI.ShowSavingMessage();
                }
            }
        }
    }



    IEnumerator wait()
    {
        if (LoadMapName.Instance == null || LoadMapName.Instance.ContinuePoint_ == null)
        {
            yield break;
        }

        LoadMapName.Instance.ContinuePoint_.SetActive(true);
        yield return new WaitForSeconds(5);
        LoadMapName.Instance.ContinuePoint_.SetActive(false);


    }
}