using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGet : MonoBehaviour
{
   
    public float BoltSpeed;
    public Transform player;

    bool _isFollowing = false;
    Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        player = Transform.FindObjectOfType<Transform>(CompareTag("Bolt"));
        _isFollowing = true;
    }


   



    




    private void Update()
    {
        if(_isFollowing)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, BoltSpeed * Time.deltaTime);
        }
    }





}
