using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class BossHealth : MonoBehaviour
{

    public static BossHealth BossHealth_;
    public Image HealthSlider;
    public float CurrentHealth;
    float maxHealth;
    public GameObject Boss;

    // Start is called before the first frame update
    void Start()
    {
        BossHealth_ = GetComponent<BossHealth>();
        
    }

    // Update is called once per frame
    void Update()
    {

        if(EnemiesHealth.EnemieHealth_.BossHealth)
        {

            Boss.SetActive(true);
            CurrentHealth = EnemiesHealth.EnemieHealth_.health;
            maxHealth = EnemiesHealth.EnemieHealth_.maxHealth;
            HealthSlider.fillAmount = CurrentHealth / maxHealth;

        }
        else
        {
            Boss.SetActive(false);
        }

        if(CurrentHealth < 600)
        {

        }

        if(CurrentHealth < 1)
        {
            Boss.SetActive(false);
        }
    }
}
