using System.Collections;
using UnityEngine;
using Cinemachine;

public class VendingMenu : MonoBehaviour
{
    public static VendingMenu Instance;

    [Header("UI & Camera Controls")]
    public GameObject vendingUI;
    public GameObject iosUI;
    public GameObject activeVendor; // Tillagd så VendorTrigger kan slå på/av denna
    public Animator anime;

    private CinemachineFreeLook freeLookCamera;
    private AllGameData gameData;
    public bool isVendingOpen { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        gameData = FindObjectOfType<AllGameData>();
    }

    private void Update()
    {
        if (freeLookCamera != null && anime != null)
        {
            anime.SetBool("Vendor", !freeLookCamera.enabled);
        }

        // Stäng shopen med Escape om den är öppen
        if (isVendingOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitVending();
        }
    }

    public void EnterVending()
    {
        isVendingOpen = true;

        if (freeLookCamera != null) freeLookCamera.enabled = false;
        if (gameData != null) gameData.DisablePlayerDo();

       MenuMouseLocked.instance.Unlocked();

        if (VendingShop.Instance != null)
        {
            //VendingShop.Instance.RefreshWeaponList();
        }

        if (vendingUI != null) vendingUI.SetActive(true);
        Time.timeScale = 0f; // Pausa spelet
    }

    public void ExitVending()
    {
        Time.timeScale = 1f; // Återuppta spelet
        isVendingOpen = false;

        if (vendingUI != null) vendingUI.SetActive(false);
        if (freeLookCamera != null) freeLookCamera.enabled = true;
        if (gameData != null) gameData.EnablePlayerDo();

        MenuMouseLocked.instance.Locked();

        if (VendorTrigger.Instance != null && VendorTrigger.Instance.anime != null)
        {
            VendorTrigger.Instance.anime.SetBool("Hide", false);
        }

        if (iosUI != null)
        {
            iosUI.SetActive(true);
        }
    }

    public void ShopAmmo()
    {
        if (VendingShop.Instance != null)
        {
            VendingShop.Instance.BuyAllAmmo();
        }
    }
}