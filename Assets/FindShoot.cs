using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindShoot : MonoBehaviour
{
    public float rotationTime = 3.10f;

    private float elapsedTime = 0.0f;
    public Transform head;
    public Transform headrot;
    public bool IsPatrol = true;
    Quaternion rot;

    public GalacticRangerGame RangerGame;

    // Start is called before the first frame update
    void Start()
    {
        rot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
       
        transform.position = head.transform.position;

        if(IsPatrol)
        {
           // transform.rotation = headrot.transform.rotation;
        }

        


        if (transform.rotation.y < rot.y)
        {
            //headrot.transform.rotation = Quaternion.Euler(90, 00, 00);
            //transform.rotation = headrot.transform.rotation;
        }   
        else
        {
            //headrot.transform.rotation = Quaternion.Euler(0.71f, 90, 90);
           // transform.rotation = headrot.transform.rotation;
        }
           

        
            
           
      
       
    }

    public void Shoot()
    {
        GetComponentInParent<GalacticRangers>().Shoot();
    }

    public void Throw()
    {
        RangerGame.ThrowGranade();
    }
}
