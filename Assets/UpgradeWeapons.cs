using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UpgradeWeapons : MonoBehaviour
{
    public Transform[] Weapons;
    public int WeaponSelect = 0;

    public GameObject whitescene;

    public GameObject Blue;
    public GameObject Red;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(wait());
    }

    // Update is called once per frame
    void Update()
    {
        WeaponSelect = GameObject.FindObjectOfType<WeaponSwitcher>().WeaponSelecter;


        int Selected = WeaponSelect;
        int i = 0;

        foreach (Transform weapon in Weapons)
        {

            if (i == WeaponSelect)
            {
                weapon.gameObject.SetActive(true);
               

            }
            else
            {
                weapon.gameObject.SetActive(false);
                


            }

            i++;
        }

        
            
        
    }


    IEnumerator wait()
    {

       
        yield return new WaitForSecondsRealtime(1);

        whitescene.SetActive(true);
        yield return new WaitForSecondsRealtime(1);
        Blue.SetActive(false);
        Red.SetActive(true);
        whitescene.SetActive(false);

        yield return new WaitForSecondsRealtime(2);
        Blue.SetActive(true);
        Red.SetActive(false);
        // StopCoroutine(wait());

    }

}
