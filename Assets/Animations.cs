using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{

    public TorretGun gun;
    
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }



    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            
        }
    }

    public void Throw()
    {
        gun.ThrowBalls();

        
    }

   


}
