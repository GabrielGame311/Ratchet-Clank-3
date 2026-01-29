using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firing_aquatosEnemie : MonoBehaviour
{
    public Animator anime;
    public ParticleSystem particle;
    public EnemiesHealth health;
    public bool Isfiring = false;
    public Bridge hacker;
    public Transform CrackedObj;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Isfiring == true)
        {
            anime.SetBool("Fireing", true);
            particle.Play();
        }

       
    }

    private void OnDestroy()
    {
        Instantiate(CrackedObj, transform.position, transform.rotation);
        hacker.hacker = true;
    }

}
