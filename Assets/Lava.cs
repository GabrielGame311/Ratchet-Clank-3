using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{

    GameObject Player_;

    int TriggerDie = 3;
    public float JumpSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(TriggerDie < 0)
        {
            Player_.GetComponent<Player>().Dielava = true;
            Player_.GetComponent<Player>().TakeDamage(120);
            
            
        }

        if (Player_.GetComponent<CharacterController>().isGrounded)
        {
            TriggerDie = 3;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (Player_.GetComponent<CharacterController>().isGrounded)
            {
                TriggerDie = 3;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            TriggerDie--;
            Player_ = other.gameObject;

            other.GetComponent<RatchetController>().anime.SetTrigger("Damage");
            other.GetComponent<Player>().TakeDamage(15);
            if (TriggerDie > 0)
            {
                
                
                other.GetComponent<RatchetController>().Jump(JumpSpeed);
            }
            
            
            


        }
    }

}
