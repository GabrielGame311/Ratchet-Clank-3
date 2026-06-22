using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSetActive : MonoBehaviour
{
    public GameObject Enemie;

    public GameObject SceneActive;

    public MonoBehaviour[] ScriptsEnabled;

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
        if(other.tag == "Player")
        {
            SceneActive.SetActive(true);
            
        }
        if(other.tag == "Player")
        {
            foreach(MonoBehaviour pl in ScriptsEnabled)
            {
                pl.enabled = true;
            }
        }
    }

    public void EnemieActive()
    {
        Enemie.SetActive(true);
    }
}
