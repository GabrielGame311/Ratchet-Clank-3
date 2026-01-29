using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBullet : MonoBehaviour
{


   
 

  
    public int damage;

    private GameObject shoot;

    private Aquatos_shooting AquatosShooting;


    // Start is called before the first frame update
    void Start()
    {

       

       
        AquatosShooting = GameObject.FindObjectOfType<Aquatos_shooting>();
    }

    // Update is called once per frame

    private void Update()
    {
       
    }



    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Player")
        {
            col.gameObject.GetComponent<Player>().TakeDamage(damage);


            
            Destroy(shoot.gameObject);
        }

        Destroy(shoot.gameObject);
    }


   

}
