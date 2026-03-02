using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThyrraNoid2 : MonoBehaviour
{


    public float MoveSpeed;

    public float Distance;
    public Animator anime;
    GameObject player;
    public float ShootTime;
    float startShoot;
    public bool SePlayer = false;
    public int SoundplayInt = 0;
    public bool Shooting = false;
    AudioSource sound;
    public GameObject Gun;
    public AudioClip[] SoundFX;
    public float DistanceFromPlayer;
    public float minDistanceFromFirstEnemy;
    public GameObject ParticlePrefab;
    public Transform pointGun;
    public float ShootForce;
    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        anime = GetComponentInChildren<Animator>();
        startShoot = ShootTime;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemie");

        


        foreach (GameObject enemy in enemies)
        {


            float distances = Vector3.Distance(transform.position, enemy.transform.position);

            if (distances < minDistanceFromFirstEnemy)
            {
                Vector3 direction = (enemy.transform.position - transform.position).normalized;
                Vector3 newPosition = transform.position + direction * minDistanceFromFirstEnemy;
                enemy.transform.position = newPosition;
            }
        }

        float dis = Vector3.Distance(transform.position, player.transform.position);

        if(dis < Distance)
        {
            SePlayer = true;
            
        }
        else
        {
            SePlayer = false;
            
        }



    
      

        if (Shooting)
        {

            if(ShootTime < 3)
            {
                Shooting = false;
                anime.SetTrigger("Shoot");
            }

            


        }
        else
        {


            if (SePlayer)
            {

                float disp = Vector3.Distance(transform.position, player.transform.position);


                if (DistanceFromPlayer < disp)
                {
                    transform.position = Vector3.MoveTowards(transform.position, player.transform.position, MoveSpeed * Time.deltaTime);
                    transform.LookAt(player.transform);
                    anime.SetBool("Run", true);



                }
                else
                {
                    anime.SetBool("Run", false);
                    transform.LookAt(player.transform);

                    SePlayer = false;
                }
                ShootTime -= Time.deltaTime;

                if (ShootTime < 0)
                {

                    ShootTime = startShoot;
                    Shooting = true;
                }


            }
            else
            {



            }
        }

    }


    public void Shoot()
    {
        SoundplayInt = 0;
        sound.PlayOneShot(SoundFX[SoundplayInt]);
      


    }

    public void ShootParticle()
    {
        GameObject prefab = Instantiate(ParticlePrefab, pointGun.transform.position, pointGun.transform.rotation);

        prefab.GetComponent<Rigidbody>().linearVelocity = pointGun.transform.forward * ShootForce;

        ParticlePrefab.GetComponent<ParticleSystem>().Play();


        Destroy(prefab, 15);
    }

    public void Glad()
    {
        SoundplayInt = 1;
        sound.PlayOneShot(SoundFX[SoundplayInt]);
    }
}
