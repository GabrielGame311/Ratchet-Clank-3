using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelWeapon : MonoBehaviour
{

    public Image Level_Slider;
    public GameObject upgrade;
   
    public TMP_Text levelcount;
    public static LevelWeapon levelWeapon_;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Level_Slider.fillAmount = 0;

        levelWeapon_ = GetComponent<LevelWeapon>();
        
    }

    // Update is called once per frame
    void Update()
    {
        levelcount.text = "V" + GameObject.FindObjectOfType<WeaponsUI>().Level.ToString();

        Level_Slider.fillAmount = GameObject.FindObjectOfType<WeaponsUI>().levelAmount;


        if (GameObject.FindObjectOfType<WeaponsUI>().Level < 5)
        {
            if (Level_Slider.fillAmount == 1)
            {
                upgrade.SetActive(true);
                //WeaponsUI.WeaponsUI_.Level += 1;

                //Level_Slider.fillAmount = 0;
                //WeaponsUI.WeaponsUI_.levelAmount = 0;
                StartCoroutine(wait());
            }
        }


             
        
        
        
          
        
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(3);
        //Level_Slider.fillAmount += WeaponsUI.WeaponsUI_.levelAmount;
        upgrade.SetActive(false);
    }


    public void levelWeapon()
    {
       
            Level_Slider.fillAmount += WeaponsUI.WeaponsUI_.levelAmount;
        


       
           
    }
     
    
}
