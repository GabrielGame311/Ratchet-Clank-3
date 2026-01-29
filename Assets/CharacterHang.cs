using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHang : MonoBehaviour
{
    public Transform hangPoint;
    public Animator animator;
    public CharacterController characterController;
    public Transform player;
    public bool canHang = false;
    public bool isHanging = false;
    public Transform point;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HangPoint"))
        {
            canHang = true;
            hangPoint = other.transform;

        }
       
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("HangPoint"))
        {
            canHang = false;
           
        }

    }

    void Update()
    {
        if (canHang)
        {
            StartHanging();
        }
        else if (isHanging && Input.GetKeyDown(KeyCode.Space))
        {
            StopHanging();
            GameObject.FindObjectOfType<QuarkController>().Jumps2();
        }
    }

    void StartHanging()
    {
        isHanging = true;
        player.transform.position = hangPoint.position;
        characterController.enabled = false;
        animator.SetBool("Hang", true);
        canHang = false;
    }

    void StopHanging()
    {
        characterController.enabled = true;
        animator.SetBool("Hang", false);
        isHanging = false;
    }
}
