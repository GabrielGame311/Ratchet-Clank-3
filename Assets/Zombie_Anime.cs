using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_Anime : MonoBehaviour
{
    public Aquatos_Zombie zombie;
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void takedamage()
    {
        zombie.damages();
    }
}
