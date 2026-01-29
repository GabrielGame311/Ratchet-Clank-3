using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarkWeapon : MonoBehaviour
{

    public Transform Weapon;
    public Transform Hand;

    public Animator anime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Weapon")
        {

            Transform pl = Instantiate(Weapon, Hand.transform);
            anime.SetBool("Gun", true);
            Destroy(other.gameObject);
        }
    }
}
