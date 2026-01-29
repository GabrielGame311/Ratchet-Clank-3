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
    float time = 5;
    bool Pressed = false;
    private static bool wasSceneLoaded = false;
    public static bool isDeathReload = false;
    public PlayableDirector LandingTimeline;

    bool IsTrigger = false;


    // Start is called before the first frame update
    void Start()
    {
       
        shipmenutrigger_ = GetComponent<ShipMenuTrigger>();
        player = GameObject.FindGameObjectWithTag("Playerholder");
       
        
        //player.SetActive(false);
        //LandingTimeline.enabled = true;
        // LandingTimeline.Play();

    }

    public static void LoadShipScene()
    {
        wasSceneLoaded = true;
       
    }

    public static void ReloadSceneOnDeath()
    {
        wasSceneLoaded = false; // Still technically loaded via SceneManager
        isDeathReload = true;  // Mark as death reload
       
    }

    void OnEnable()
    {
        // Subscribe to the sceneLoaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if this is the correct scene and loaded in single mode
        if (scene.name == SceneManager.GetActiveScene().name && mode == LoadSceneMode.Single)
        {
           LandingShip();
        }
    }
    // Update is called once per frame
    void Update()
    {

       
       

       

    }

    public void LandingShip()
    {
        //player.SetActive(false);

        ShipMenu.ShipMenu_.BackButton();
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            ShipMenu.ShipMenu_.ItsTrigger = true;
            IsTrigger = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.tag == "Player")
        {
            ShipMenu.ShipMenu_.ItsTrigger = false;
            IsTrigger = false;

        }
    }
}
