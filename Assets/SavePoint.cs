using System.Collections;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnLocation;
    
    private bool isActivated = false;

    void Start()
    {
        if (spawnLocation == null) spawnLocation = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;

            // Spara ENBART i TempCheckpoint i RAM-minnet
            TempCheckpoint.SetCheckpoint(spawnLocation.position, spawnLocation.rotation);

            // Visa UI-meddelande i 10 sekunder
            StartCoroutine(ShowContinuePointForTenSeconds());
        }
    }

    private IEnumerator ShowContinuePointForTenSeconds()
    {
        if (LoadMapName.Instance != null && LoadMapName.Instance.ContinuePoint_ != null)
        {
            LoadMapName.Instance.ContinuePoint_.SetActive(true);
        }

        yield return new WaitForSeconds(10f);

        if (LoadMapName.Instance != null && LoadMapName.Instance.ContinuePoint_ != null)
        {
            LoadMapName.Instance.ContinuePoint_.SetActive(false);
        }
    }
}