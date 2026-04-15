using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaRed : MonoBehaviour
{
    public RedNinja redninja_;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot()
    {
        redninja_.Shooting();
    }

     
    public void Run()
    {
        redninja_.Isrunning = true;
    }
    public void RunFalse()
    {
        redninja_.Isrunning = false;
    }
}
