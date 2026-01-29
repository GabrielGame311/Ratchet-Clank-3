using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedNinja : MonoBehaviour
{
    public bool DiscNinja_;
    public float MoveSpeed;

    public float DamageTime;
    float StartTime;
    public int TakeDamage;
    public float ShootSpeed;
    public Transform ShootPoint;
    public GameObject prefabshoot;
    Animator anime;

    public float AttackDistance;
    public float FromPlayerDistance;

    GameObject Player;

    public bool SePlayer = false;

    float minDistanceFromFirstEnemy = 4;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        anime = GetComponentInChildren<Animator>();
        
        StartTime = DamageTime;
        if(DiscNinja_)
        {
            anime.SetBool("idleDisc", true);
        }
       
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


        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        float mindis = Vector3.Distance(transform.position, Player.transform.position);


        if(FromPlayerDistance < mindis)
        {

            SePlayer = false;
        }
        else
        {
            
            SePlayer = true;
        }

        float DistanceAttack = Vector3.Distance(transform.position, Player.transform.position);

        if (DistanceAttack < AttackDistance)
        {
            if(DiscNinja_ == false)
            {
                anime.SetBool("Run", false);
                DamageTime -= Time.deltaTime;

                if (DamageTime < 0)
                {

                    TakeDamages();

                    DamageTime = StartTime;

                }
            }

        }
        else
        {
            if (SePlayer)
            {

                if(DiscNinja_ == false)
                {
                    transform.position = Vector3.MoveTowards(transform.position, Player.transform.position, MoveSpeed * Time.deltaTime);


                    anime.SetBool("Run", true);
                }
                else
                {
                    DamageTime -= Time.deltaTime;
                    Vector3 direction = Player.transform.position - ShootPoint.position;
                    direction.y = 0; // Ignore the y-axis to prevent vertical rotation

                    if (direction != Vector3.zero) // Ensure the direction is not zero
                    {
                        Quaternion rotation = Quaternion.LookRotation(direction);
                        ShootPoint.rotation = rotation;
                    }
                    if (DamageTime < 0)
                    {

                        Shoot();

                        DamageTime = StartTime;

                    }
                }
               
                transform.LookAt(Player.transform);

            }
            else
            {

                anime.SetBool("Run", false);

            }

        }

       



       

    }



    void TakeDamages()
    {

        anime.SetTrigger("Attack");
        Player.GetComponent<Player>().TakeDamage(TakeDamage);
        




    }
    public void Shoot()
    {
       
        anime.SetTrigger("Attack");

      



    }

    public void Shooting()
    {

        GameObject prefabs = Instantiate(prefabshoot, ShootPoint.transform.position, ShootPoint.transform.rotation);
        prefabs.GetComponent<Rigidbody>().velocity = ShootPoint.transform.forward * ShootSpeed;

        Destroy(prefabs, 5);
    }
}
