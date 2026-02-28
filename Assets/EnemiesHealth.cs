using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

public class EnemiesHealth : MonoBehaviour
{
    public float colorChangeDuration = 0.2f;
   
    public Renderer[] MaterialRed;
    public int health = 1;
    public int maxHealth = 0;
    private GameObject enemie;
    public Animator anime;
    private AudioSource sound;
    public AudioClip clipsound;
    public bool destroy = false;
    public GameObject Bolt;
    public float knockbackForce = 2;
    public Transform ExplodePrefab;
    bool play = false;
    public AudioClip SoundDamage;
    public bool BossHealth = false;
    public float LevelXp = 0.5f;
    public static EnemiesHealth EnemieHealth_;
    public Color damageColor = Color.red;         // Färg för skada (röd)
    public Color startColor = Color.white;        // Ursprunglig färg
    public Animator animes;
    private void Start()
    {
    
        MaterialRed = GetComponentsInChildren<Renderer>();

      


        if (BossHealth)
        {
            EnemieHealth_ = GetComponent<EnemiesHealth>();
        }

        StartCoroutine(Wait());
        anime = GetComponentInChildren<Animator>();
        //transform.parent = null;
        maxHealth = health;
        //gameObject.SetActive(false);

       
        enemie = GetComponent<GameObject>();

        
        sound = GetComponent<AudioSource>();


        animes = animes.GetComponent<Animator>();

    }

    private void Update()
    {




       

    }

    private IEnumerator ChangeColorTemporarily()
    {
        // Ändrar färgen till skadans färg för alla material på alla Renderer-komponenter
        foreach (Renderer renderer in MaterialRed)
        {
            foreach (Material mat in renderer.materials)
            {
                mat.color = damageColor;  // Sätter materialets färg till röd
            }
        }

        // Väntar under en viss tid
        yield return new WaitForSeconds(colorChangeDuration);

        // Återställer färgen till ursprungsfärgen för alla material
        foreach (Renderer renderer in MaterialRed)
        {
            foreach (Material mat in renderer.materials)
            {
                mat.color = startColor;  // Återställ till startfärgen
            }
        }
    }


    public void TakeDamage(int damage)
    {
        health -= damage;



        StartCoroutine(ChangeColorTemporarily());





        if (health <= 0)
        {

            health = 0;

            Die();
            
        }
        else
        {
            sound.PlayOneShot(SoundDamage);
        }
        if(damage > 2)
        {
            anime.SetTrigger("Damage");

        }
        animes.SetTrigger("DamageRed");

    }


    void OnEnable()
    {
        if (EnemiesMission.instance != null)
        {
           // EnemiesMission.instance.EnemiesList.Add(gameObject);
        }
    }

    public void Die()
    {
        if(anime != null)
        {
            anime.SetTrigger("Die");
        }
       

        if(GetComponent<BloodFly>() != null)
        {
            GetComponent<BloodFly>().enabled = true;
        }

        

        
            
        
       
            
        
           
        

        Destroy(gameObject, 0.2f);


       





       

        if (GetComponent<MiniThyrra>() != null)
        {
            GetComponent<MiniThyrra>().enabled = false;
        }
        else if (GetComponent<RedNinja>() != null)
        {
            GetComponent<RedNinja>().enabled = false;
        }

       

       
    }

    
    

    private void OnDestroy()
    {
        // LevelWeapon.levelWeapon_.levelWeapon();
        //LevelXp += WeaponsUI.WeaponsUI_.levelAmount;

        //WeaponsUI.WeaponsUI_.levelAmount += LevelXp;
        Instantiate(Bolt, transform.position, transform.rotation);

        if (EnemiesMission.instance.gameObject.activeSelf == true)
        {
            EnemiesMission.instance.EnemiesList.Remove(gameObject);
        }

        if (RocketMission.RocketMission_.gameObject.activeSelf == true)
        {
            RocketMission.RocketMission_.DropShip.Remove(gameObject);
        }
        if (RocketMission.RocketMission_.gameObject.activeSelf == true)
        {
            RocketMission.RocketMission_.Enemies.Remove(gameObject);
        }
        if (RocketMission.RocketMission_.gameObject.activeSelf == true)
        {
            RocketMission.RocketMission_.Rockets.Remove(gameObject);
        }




        WeaponsUI ui = FindObjectOfType<WeaponsUI>();
        if (ui != null)
        {
            ui.levelAmount += LevelXp;
        }

        if (GameObject.FindObjectOfType<SpawnTime>().DropshipsSpawned != null)
        {
            GameObject.FindObjectOfType<SpawnTime>().DropshipsSpawned.Remove(gameObject);
        }
        if(GameObject.FindObjectOfType<SpawnTime>().EnemiesSpawned != null)
        {
            GameObject.FindObjectOfType<SpawnTime>().EnemiesSpawned.Remove(gameObject);
        }

        
        

       

        if (ExplodePrefab != null)
        {
            Transform exp = Instantiate(ExplodePrefab, transform.position, transform.rotation);
            Destroy(exp.gameObject, 2);
            foreach (Rigidbody gm in exp.GetComponentsInChildren<Rigidbody>())
            {
                gm.AddExplosionForce(10, transform.position, 5);
            }

            
        }
        


        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        // Iterate through all scripts and disable them
        foreach (MonoBehaviour script in scripts)
        {
            // Ensure we don't disable the DisableAllScripts script itself
            if (script != this)
            {
                script.enabled = false;
            }
        }




























        foreach (GalacticRangers gl in GameObject.FindObjectsOfType<GalacticRangers>())
        {
            gl.IsShooting = false;

        }
       
       
        
    }

    public void takedamage()
    {
        sound.PlayOneShot(clipsound);
    }
  

    IEnumerator Wait()
    {

        GetComponent<ThyrranoidLaser>().SePlayer = false;


        yield return new WaitForSeconds(3);
        GetComponent<ThyrranoidLaser>().SePlayer = true;


    }

}
