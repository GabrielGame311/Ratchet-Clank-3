using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rangerwalkanime : MonoBehaviour
{

    private Animator anime;
    public Transform[] waypoints;
    
    public int speed;
    private int waypointIndex;
    private float dist;

    private void Start()
    {
        waypointIndex = 0;
        transform.LookAt(waypoints[waypointIndex].position);
        anime = GetComponent<Animator>();
        
       
    }

    void Update()
    {
        dist = Vector3.Distance(transform.position, waypoints[waypointIndex].position);
        if (dist < 1f)
        {
            IncreaseIndex();

        }
        Patroll();
    }
    void Patroll()
    {
        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    void IncreaseIndex()
    {
        waypointIndex++;
        if (waypointIndex >= waypoints.Length)
        {
            waypointIndex = 0;
        }
        transform.LookAt(waypoints[waypointIndex].position);

    }




}
