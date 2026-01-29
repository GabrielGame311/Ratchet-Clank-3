using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

  
    
    public static Player Player_;
    public Animator anime;
    public Animator fade;
    private CharacterController controller;
    public string SceneLoad;
    float st = 0;
    public bool Dielava = false;
    private void Start()
    {



        Player_ = GetComponent<Player>();

        fade = GameObject.Find("fade").GetComponent<Animator>();



        maxHealth = currentHealth;
        



        
        

        controller = GetComponent<CharacterController>();
    }






    private void Update()
    {

       HealthBar.HealthBar_.SetMaxHealth(maxHealth);
        HealthBar.HealthBar_.SetHealth(currentHealth);
       // HealthBar.HealthBar_.HealthText_.text = currentHealth.ToString() + " / " + maxHealth.ToString();


       HealthBar.HealthBar_.HealthText_.text =
        $"<size={HealthBar.HealthBar_.Front_HPSize}>{currentHealth.ToString("D")}</size> " +
        $"<size={HealthBar.HealthBar_.Front_MaxHPSize}>/{maxHealth.ToString("D")}</size>";


        




        if (transform.position.y < -1)
        {
            Fall();
        }


        float threshold = GetHealthWarningThreshold(maxHealth);

        if (currentHealth < threshold)
        {
            HealthBar.HealthBar_.GetComponent<Animation>().Play();
        }


    }

    float GetHealthWarningThreshold(float maxHealth)
    {
        if (maxHealth >= 50)
            return 20; // Fixed value for most cases
        else
            return maxHealth * 0.6f; // 50% for very low max health
    }

    void Fall()
    {


        Deads();
        
        anime.SetTrigger("Fall");
    }

    public void TakeDamage(int damage)
    {
        int reducedDamage = damage;

        switch (GameObject.FindObjectOfType<AllGameData>().Armor)
        {
            case 1:
                reducedDamage -= 2;
                break;
            case 2:
                reducedDamage -= 4;
                break;
            case 3:
                reducedDamage -= 6;
                break;
            case 4:
                reducedDamage -= 8;
                break;
            default:
                // Armor 0 = ingen reduktion
                break;
        }

        // Skadan får aldrig vara mindre än 1
        reducedDamage = Mathf.Max(reducedDamage, 1);

        currentHealth -= reducedDamage;

        deadanime();
    }

    private int CalculateDamage(int baseDamage, int armorLevel)
    {
        // Enkel exempel-logik: varje armor-nivå minskar skadan med t.ex. 10%
        float reductionFactor = 1f - (armorLevel * 0.1f); // Armor 1 = 10%, Armor 2 = 20%, osv
        reductionFactor = Mathf.Clamp(reductionFactor, 0.1f, 1f); // undvik att reducera helt till 0

        return Mathf.CeilToInt(baseDamage * reductionFactor);
    }


    public void  GetHealth(int health)
    {
        currentHealth += health;
        HealthBar.HealthBar_.SetHealth(currentHealth);
        HealthBar.HealthBar_.HealthText_.text = currentHealth.ToString();
    }

    void deadanime()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            
            if(Dielava)
            {
                anime.SetTrigger("LavaDead");
            }
            else
            {
                controller.enabled = false;
                anime.SetTrigger("Dead");
            }
            StartCoroutine(dead());
        }


        
    }

    public void Deads()
    {

        StartCoroutine(dead());
       
    }



    IEnumerator dead()
    {

        
        yield return new WaitForSeconds(3);

        fade.SetBool("Fade", true);
        yield return new WaitForSeconds(1.1f);

        ShipMenuTrigger.ReloadSceneOnDeath();
        if (SceneLoad == null)
        {
            SceneManager.LoadScene(SceneLoad);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        fade.SetBool("Fade", false);

    }


}
