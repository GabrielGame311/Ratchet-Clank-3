using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class ShipMenuTrigger : MonoBehaviour
{
    public PlayableDirector StartShip;
    public static ShipMenuTrigger shipmenutrigger_;
    public GameObject player;
    public GameObject cameras;
    public PlayableDirector Flytime;
    public AudioSource Music;
    
    private static bool wasSceneLoaded = false;
    public static bool isDeathReload = false;
    public PlayableDirector LandingTimeline;

    bool IsTrigger = false;

    private void Awake()
    {
        // Sätt Singleton i Awake istället för Start så att den finns tillgänglig direkt
        shipmenutrigger_ = this;
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Playerholder");
        }

        // Om scenen laddades efter en skeppsresa, starta landnings-sekvensen
        if (wasSceneLoaded && !isDeathReload)
        {
            wasSceneLoaded = false; // Återställ flaggan
            StartCoroutine(StartLandingNextFrame());
        }
    }

    public static void LoadShipScene()
    {
        wasSceneLoaded = true;
        isDeathReload = false;
    }

    public static void ReloadSceneOnDeath()
    {
        wasSceneLoaded = false;
        isDeathReload = true;
    }

    /// <summary>
    /// Väntar 1 bildruta så att alla andra skript (t.ex. ShipMenu) hinner köra sin Start()
    /// </summary>
    private IEnumerator StartLandingNextFrame()
    {
        yield return null;

        if (ShipMenu.ShipMenu_ != null)
        {
            LandingShip();
        }
        else
        {
            // Om ShipMenu inte finns i denna scen, spela upp timeline direkt
            if (LandingTimeline != null)
            {
                if (player != null) player.SetActive(false);
                LandingTimeline.enabled = true;
                LandingTimeline.Play();
            }
        }
    }

    public void LandingShip()
    {
        if (ShipMenu.ShipMenu_ != null)
        {
            ShipMenu.ShipMenu_.BackButton();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && ShipMenu.ShipMenu_ != null)
        {
            ShipMenu.ShipMenu_.ItsTrigger = true;
            IsTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && ShipMenu.ShipMenu_ != null)
        {
            ShipMenu.ShipMenu_.ItsTrigger = false;
            IsTrigger = false;
        }
    }
}