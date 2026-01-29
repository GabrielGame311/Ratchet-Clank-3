using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{

    int box = 1;
    int maxbox;

    public Transform Cracked;
    private GameObject obj;
    public Transform Bolt;

    public GameObject spawn;
    // Start is called before the first frame update
    void Start()
    {
        box = maxbox;
        obj = GetComponent<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }


    private void OnDestroy()
    {
        Instantiate(Bolt, spawn.transform.position, transform.rotation);
        Instantiate(Cracked, transform.position, transform.rotation);
    }

    public void Break(int damage)
    {
        box -= damage;
        
        
        if (box <= 0)
        {
            Destroy(gameObject);
            
            
            
        }

        
    }
}
