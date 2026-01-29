using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarController : MonoBehaviour
{

    GameObject Player;

    public float DriveSpeed;
    public Transform Camera_;

    public bool IsCar = false;
    public Transform Pos;
    public Transform StartPos;
    public float RotationSpeed;
    float xRotation = 0;
    float yRotation = 0;
    bool IsTrigger = false;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");


    }

    // Update is called once per frame
    void Update()
    {

        if(IsCar)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // Calculate the movement vector
            Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput) * DriveSpeed * Time.deltaTime;

            // Apply the movement to the object's position
            transform.Translate(movement);


            float mouseX = Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime;
           



            
            
            yRotation += mouseX;




            transform.localRotation = Quaternion.Euler(0, yRotation, 0);

        }


        if(IsTrigger)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {

                if (IsCar)
                {
                    Camera_.gameObject.SetActive(false);
                    Player.transform.position = StartPos.transform.position;
                    IsCar = false;
                    Player.GetComponentInChildren<CinemachineFreeLook>().gameObject.SetActive(true);
                    Player.GetComponentInChildren<RatchetController>().enabled = true;
                    Player.GetComponentInChildren<Animator>().SetBool("Car", false);
                    Player.transform.parent = null;
                    Player.GetComponent<CharacterController>().enabled = true;
                   
                   
                }
                else
                {
                    IsCar = true;
                    Player.GetComponent<CharacterController>().enabled = false;
                    
                    
                    Player.transform.position = Pos.transform.position;
                    Player.transform.rotation = Pos.transform.rotation;
                    Player.transform.parent = Pos;
                    Camera_.gameObject.SetActive(true);
                    Player.GetComponentInChildren<CinemachineFreeLook>().gameObject.SetActive(false);
                    Player.GetComponentInChildren<RatchetController>().enabled = false;
                    Player.GetComponentInChildren<Animator>().SetBool("Car", true);
                }

            }

        }

       

        // Get input axes for horizontal and vertical movement
      



    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {


            IsTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {

            IsTrigger = false;

        }

    }
}
