using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSpawn : MonoBehaviour
{

    public float TimeLoad;
    float timestart;
    Transform Spawned;
    public Transform Point;
    public Transform AmmoPrefab;
    bool CanSpawn = true;
    Animator anime;

    // Start is called before the first frame update
    void Start()
    {

        anime = GetComponent<Animator>();
        timestart = TimeLoad;
       // anime.SetTrigger("Spawn");
    }

    // Update is called once per frame
    void Update()
    {
        
        if(CanSpawn == true)
        {
            TimeLoad -= Time.deltaTime;


            if(TimeLoad < 0)
            {

                anime.SetTrigger("Spawn");
                TimeLoad = timestart;
            }
        }
        

        if(Spawned == null)
        {
            CanSpawn = true;
        }
        else
        {
            CanSpawn = false;
        }

    }

   

    public void SpawnAmmo()
    {

       Transform ammo = Instantiate(AmmoPrefab, Point.transform.position, Point.transform.rotation);

        ammo.transform.parent = Point.transform;
        Spawned = ammo;
    }
}
