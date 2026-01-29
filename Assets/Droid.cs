using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Droid : MonoBehaviour
{
    public GameObject Particle;
    public Transform point;

    public Animator[] anime;

    public float ShootTime;
    public float Speed;
    public float PlayerDistance;
    public float MoveSpeed;
    public float ShootForce;
    float StartShoot;
    GameObject Player;


    bool SePlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        StartShoot = ShootTime;
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {

        foreach(Animator animes in anime)
        {
            float dis = Vector3.Distance(transform.position, Player.transform.position);

            if (dis < PlayerDistance)
            {
                SePlayer = true;

            }
            else
            {
                SePlayer = false;
            }

            if (SePlayer)
            {
                animes.SetBool("SePlayer", true);

                transform.LookAt(Player.transform);
                ShootTime -= Time.deltaTime;

                if(ShootTime < 0)
                {

                    animes.SetTrigger("Shoot");
                    ShootTime = StartShoot;
                }
            }
            else
            {
                animes.SetBool("SePlayer", false);

                

            }




        }


    }


    public void Shoot()
    {
        GameObject prefab = Instantiate(Particle, point.transform.position, point.transform.rotation);

        prefab.GetComponent<Rigidbody>().velocity = prefab.transform.forward * ShootForce;

        Destroy(prefab, 10);
    }


}
