using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendorTrigger : MonoBehaviour
{

    public bool Istrigger = false;
    public Animator anime;
    public static VendorTrigger vendortrigger_;
    // Start is called before the first frame update
    void Start()
    {
        vendortrigger_ = GetComponent<VendorTrigger>();
    }

    // Update is called once per frame
    void Update()
    {


        if (Istrigger)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                VendingMenu.VendingMenu_.EnterVending();
                anime.SetBool("Hide", true);
            }
        }




    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
           
            Istrigger = true;
            UpdateVendingMenu();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            
            Istrigger = false;
            UpdateVendingMenu();
        }
    }


    void UpdateVendingMenu()
    {
        if(VendingMenu.VendingMenu_.ActiveVendor.gameObject != null)
        {
            
            VendingMenu.VendingMenu_.ActiveVendor.SetActive(Istrigger);
            
           // anime.SetBool("Hide", Istrigger);
        }
    }
}
