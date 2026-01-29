using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : MonoBehaviour
{
    GameObject player;
    public Transform[] railPoints;  // Waypoints for the rail
    public float grindSpeed = 10f;   // Speed along the rail
    public bool isTrigger = false;
    public Transform playerpos;
    public float verticalOffset = -1f;
    public GameObject Ratchet_;
    private CharacterController playerController;
    Camera cam;
    public Vector3 hangOffset = new Vector3(0, -1, 0); // Offset to hang under rail

    
    private int currentPoint = 0;
   

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<CharacterController>();
        Ratchet_ = GameObject.FindGameObjectWithTag("Ratchet");
        cam = GameObject.Find("GlideCAm").GetComponent<Camera>();
    }

    void Update()
    {
       


            MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
            // Iterate through all scripts and disable them
            foreach (MonoBehaviour script in scripts)
            {
                // Ensure we don't disable the DisableAllScripts script itself
               

                if(isTrigger)
                {

                    if (script != this)
                    {
                        script.enabled = false;
                    }
                    cam.enabled = true;

                    // Disable CharacterController for direct movement
                    playerController.enabled = false;
                    
                    GameObject.FindObjectOfType<WeaponSwitcher>().WrenchEnable();
                    Vector3 targetPos = railPoints[currentPoint].position + hangOffset;
                    player.transform.position = Vector3.MoveTowards(player.transform.position, targetPos, grindSpeed * Time.deltaTime);
                    player.transform.LookAt(targetPos);
                    if (Vector3.Distance(player.transform.position, targetPos) < 0.1f && currentPoint < railPoints.Length - 1)
                    {
                        currentPoint++;
                    }
                    else if (currentPoint == railPoints.Length - 1)
                    {
                        isTrigger = false;
                        playerController.enabled = true;
                        Ratchet_.GetComponent<Animator>().SetBool("Glide", false);
                        GameObject.FindObjectOfType<WeaponSwitcher>().WeaponSwitch();
                    }







               
                }
                else
                {
                    cam.enabled = false;
                    if (script != this)
                        {
                        script.enabled = true;
                    }
                    

                }

               
            }
       
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTrigger = true;
            
            
            currentPoint = 0;
            Ratchet_.GetComponent<Animator>().SetBool("Glide", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
             //isTrigger = false;
            // playerController.enabled = true;
             //Ratchet_.GetComponent<Animator>().SetBool("Glide", false);
            // GameObject.FindObjectOfType<WeaponSwitcher>().WeaponSwitch();
            
        }
    }
}
