using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleExplod : MonoBehaviour
{

    public int Health;
    public int MaxHealth;

    public GameObject SceneActive;





    // Start is called before the first frame update
    void Start()
    {
        MaxHealth = Health;
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void TakeDamage(int damage)
    {

        Health -= damage;

        if(Health < 0)
        {


            Health = 0;

            Destroy(gameObject, 01);
        }
    }


    private void OnDestroy()
    {
        SceneActive.SetActive(true);
    }
}
