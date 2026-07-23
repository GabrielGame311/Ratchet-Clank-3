using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehicleEnterExit : MonoBehaviour
{
    [Header("References")]
    public CarController carController;
    public GameObject player;             // Spelarkaktären (Ratchet)
    public Camera playerCamera;           // Spelarens vanliga kamera
    public Camera carCamera;              // Bilens kamera
    public Transform exitPoint;           // Var spelaren hamnar när den hoppar UR bilen

    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E; // Knappen för att hoppa in/ur
    
    private bool isPlayerNear = false;
    private bool isDriving = false;
    public Transform PointSpawn;
    private List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
    Animator anime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponentInChildren<Animator>();
        // Se till att bilen inte går att styra från början och stäng av bilkameran
        if (carController != null) carController.isCar = false;
        if (carCamera != null) carCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        // Hoppa IN i bilen när man står bredvid och trycker E
        if (isPlayerNear && !isDriving && Input.GetKeyDown(interactKey))
        {
            EnterVehicle();
        }
        // Hoppa UR bilen när man kör och trycker E
        else if (isDriving && Input.GetKeyDown(interactKey))
        {
            ExitVehicle();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            player = other.gameObject; // Hittar spelaren automatiskt
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    void EnterVehicle()
    {
        isDriving = true;

        // 1. Dölj/avaktivera spelarmodellen
        //if (player != null) player.SetActive(false);
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = PointSpawn.position;
        player.transform.rotation = PointSpawn.rotation;
        player.transform.SetParent(transform); // Sätt spelaren som barn till bilen för att följa med
        disabledScripts.Clear();
                foreach (AnimatorControllerParameter param in anime.parameters)
                {
                    if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        anime.SetBool(param.name, false);
                    }
                }
        anime.SetBool("isDriving", true); // Starta bilkörningsanimationen
        WeaponSwitcher.WeaponSwitcher_.DeactivateWeapons(); // Inaktivera vapen när man kör bil
            foreach (var mb in player.GetComponentsInChildren<MonoBehaviour>())
            {
                // Om skriptet var igång, stäng av det och spara i listan
                if (mb != this && mb.enabled)
                {
                    mb.enabled = false;
                    disabledScripts.Add(mb);
                }
            }

        // 2. Aktivera bilstyrningen i CarController
        if (carController != null) carController.isCar = true;

        // 3. Byt till bilens kamera
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        if (carCamera != null) carCamera.gameObject.SetActive(true);
    }

    void ExitVehicle()
    {
        isDriving = false;

        // 1. Stäng av bilstyrningen
        if (carController != null) carController.isCar = false;

        // 2. Placera spelaren vid utgångspunkten och aktivera den igen
        if (player != null)
        {
            if (exitPoint != null)
            {
                player.transform.position = exitPoint.position;
            }
            else
            {
                // Om ingen point finns, placera spelaren lite till vänster om bilen
                player.GetComponent<CharacterController>().enabled = false; // Temporärt inaktivera CharacterController för att undvika kollisioner
                player.transform.position = transform.position - transform.right * 2.5f;
                player.GetComponent<CharacterController>().enabled = true;
                player.transform.SetParent(null); 
                anime.SetBool("isDriving", false); // Stoppa bilkörningsanimationen
                foreach (var mb in disabledScripts)
                {
                    if (mb != null) mb.enabled = true;
                }
                disabledScripts.Clear();
                
            }

            player.SetActive(true);
        }

        // 3. Byt tillbaka till spelarkameran
        if (carCamera != null) carCamera.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);
        WeaponSwitcher.WeaponSwitcher_.ActivateWeapons(); // Aktivera vapen när man lämnar bilen
    }
}