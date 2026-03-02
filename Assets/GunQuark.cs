using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunQuark : MonoBehaviour
{
    public float ShootSpeed;
    public float TimeShoot;
    public Transform Point;
    public GameObject Particle;
    float startTime;
    bool IsSHoots = false;

    // Start is called before the first frame update
    void Start()
    {
        startTime = TimeShoot;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsSHoots)
        {

            TimeShoot -= Time.deltaTime;

            if(TimeShoot < 0)
            {
                TimeShoot = startTime;
                IsSHoots = false;

            }
           
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
               
                Shoot();
            }
        }
    }

    void Shoot()
    {


        GameObject pr = Instantiate(Particle, Point.transform.position, Point.transform.rotation);
        pr.GetComponent<Rigidbody>().linearVelocity = Point.transform.forward * ShootSpeed;
        GameObject.FindObjectOfType<QuarkController>().anime.SetTrigger("Shoot");
        IsSHoots = true;

    }
}
