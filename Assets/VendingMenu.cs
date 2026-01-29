using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VendingMenu : MonoBehaviour
{
    public int WeaponID;
    public GameObject VendingUI;
    public GameObject Ios_UI;
    public GameObject ActiveVendor;
    public static VendingMenu VendingMenu_;
    public int Price;
    public int HavePrice = 0;
    public Animator anime;
    public int selectedWeaponIndex = 0; // Index för det valda vapnet
    public int maxWeaponIndex; // Det maximala antalet vapen (sätts i Start eller Awake)
    bool EnterVendings = false;
    public List<WeaponAmmos> weaponAmmosList = new List<WeaponAmmos>();

    // Start is called before the first frame update
    void Start()
    {
        VendingMenu_ = GetComponent<VendingMenu>();

        foreach (WeaponAmmos wp in Resources.FindObjectsOfTypeAll<WeaponAmmos>())
        {
            weaponAmmosList.Add(wp);
        }
    }

    // Update is called once per frame
    void Update()
    { 
        if (GameObject.FindObjectOfType<CinemachineFreeLook>().enabled)
        {
            anime.SetBool("Vendor", false);
        }
        else
        {
            anime.SetBool("Vendor", true);
        }

        if (Time.timeScale == 0 && EnterVendings)
        {
            // Hantera vänster-tangent ("A")
            if (Input.GetKeyDown(KeyCode.A))
            {
                selectedWeaponIndex--;
                if (selectedWeaponIndex < 0)
                {
                    selectedWeaponIndex = 0; // Förhindra att gå under 0
                }
            }

            // Hantera höger-tangent ("D")
            if (Input.GetKeyDown(KeyCode.D))
            {
                selectedWeaponIndex++;
                if (selectedWeaponIndex > maxWeaponIndex)
                {
                    selectedWeaponIndex = maxWeaponIndex; // Förhindra att gå över max
                }
            }

            
        }


       GameObject.FindObjectOfType<VendingShop>().Price = Price;

    }


    public void ShopAmmo()
    {

       

        if (selectedWeaponIndex == 0)
        {
            foreach (WeaponAmmos wp in weaponAmmosList)
            {
                GameObject.FindObjectOfType<Bolts>().bolt -= wp.havePrice;
                wp.havePrice -= wp.havePrice; // Nollställ priset (om det är logiskt i din applikation)
                wp.RefillAmmo();
            }
        }
        else if (selectedWeaponIndex > 0 && selectedWeaponIndex <= maxWeaponIndex)
        {
            WeaponAmmos selectedWeapon = weaponAmmosList[selectedWeaponIndex - 1]; // -1 för att hantera array-index
            GameObject.FindObjectOfType<Bolts>().bolt -= selectedWeapon.havePrice;
            selectedWeapon.havePrice -= selectedWeapon.havePrice;
            selectedWeapon.RefillAmmo();
        }
    }

    public void EnterVending()
    {
        EnterVendings = true;
        GameObject.FindObjectOfType<CinemachineFreeLook>().enabled = false;
        GameObject.FindObjectOfType<AllGameData>().DisablePlayerDo();

        Cursor.lockState = CursorLockMode.None;
        
        StartCoroutine(wait());

        GameObject.FindObjectOfType<WeaponAmmos>().enabled = false;

    }

    public void ExitVending()
    {
        EnterVendings = false;
        GameObject.FindObjectOfType<AllGameData>().EnablePlayerDo();
        GameObject.FindObjectOfType<CinemachineFreeLook>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        
       
        
        VendorTrigger.vendortrigger_.anime.SetBool("Hide", false);
        if (Ios_UI == null)
        {
           Ios_UI.SetActive(true);
        }

        StartCoroutine(wait2());
        GameObject.FindObjectOfType<WeaponAmmos>().enabled = true;
    }


    IEnumerator wait()
    {
        VendingUI.SetActive(true);
        yield return new WaitForSeconds(1);
        Time.timeScale = 0;
        
    }
    IEnumerator wait2()
    {

        Time.timeScale = 1;
       
        yield return new WaitForSeconds(0.6f);
        VendingUI.SetActive(false);
        
       
    }

}
