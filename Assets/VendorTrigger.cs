using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendorTrigger : MonoBehaviour
{
    public static VendorTrigger Instance;

    [Header("Settings & Animations")]
    public bool isTrigger = false;
    public Animator anime;
    public GameObject promptUI; // UI-text/ikon för "Tryck E"

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isTrigger && Input.GetKeyDown(KeyCode.E))
        {
            if (VendingMenu.Instance != null && !VendingMenu.Instance.isVendingOpen)
            {
                VendingMenu.Instance.EnterVending();

                if (anime != null)
                {
                    anime.SetBool("Hide", true);
                }

                if (promptUI != null)
                {
                    promptUI.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTrigger = true;
            UpdateVendingMenu();

            if (promptUI != null)
            {
                promptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTrigger = false;
            UpdateVendingMenu();

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            if (anime != null)
            {
                anime.SetBool("Hide", false);
            }
        }
    }

    private void UpdateVendingMenu()
    {
        if (VendingMenu.Instance != null && VendingMenu.Instance.activeVendor != null)
        {
            VendingMenu.Instance.activeVendor.SetActive(isTrigger);
        }
    }
}