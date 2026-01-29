using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrenchEnemies : MonoBehaviour
{

    
    
    public int damage;

    private GameObject wrenchobj;

    public float AttackCooldown = 01f;
    public  bool CanAttack = false;
    int s;
    private BoxCollider collidier;

    

    private EnemiesHealth health;
    // Start is called before the first frame update
    void Start()
    {
       
        wrenchobj = GetComponent<GameObject>();

        health = FindObjectOfType<EnemiesHealth>();

        collidier = GetComponent<BoxCollider>();

       


    }

    // Update is called once per frame
    void Update()
    {

        


            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                CanAttack = true;
                StartCoroutine(ResetAttack());
            }

      




    }







    private void OnTriggerEnter(Collider other)
    {
        
        
            if (other.CompareTag("Enemie") && CanAttack == true)
            {

                other.gameObject.GetComponent<EnemiesHealth>().TakeDamage(damage);

               CanAttack = false;




            }

        





    }

    

    public void WrenchAttack()
    {

        


    }
    IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = false;
    }









}
