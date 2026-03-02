using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public GameObject Prefab_Particle;
    public Transform LeftPoint;
    public Transform RightPoint;
    public float ParticleSpeed;
    public int ShootInt = 0;
    public float rotationSpeed = 5f;
    public float maxRotationDown = -80f;
    public bool IsTrigger = false;



    Animator anime;



    // Start is called before the first frame update
    void Start()
    {
        anime = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {

            Fire();
        }

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Apply rotation to the object
        // In this example, we're rotating around the object's local up and right axes
        // You can modify this based on your desired behavior

        // Rotate around the world up axis (y-axis)
        transform.Rotate(Vector3.up, -mouseX * rotationSpeed, Space.World);

        // Rotate around the object's local right axis (x-axis)
        float rotationAmount = mouseY * rotationSpeed;
        float currentRotationX = transform.localEulerAngles.x;

        // Calculate the desired new rotation on the X-axis
        float newRotationX = currentRotationX + rotationAmount;

        // Clamp the rotation to the desired range
        if (newRotationX > 180f)
        {
            newRotationX -= 360f;
        }

        newRotationX = Mathf.Clamp(newRotationX, maxRotationDown, 90f); // 90 degrees is looking straight up

        // Apply the new rotation
        transform.localEulerAngles = new Vector3(newRotationX, transform.localEulerAngles.y, transform.localEulerAngles.z);

    }


    public void Fire()
    {
        if(ShootInt == 0)
        {
            GameObject particle = Instantiate(Prefab_Particle, LeftPoint.transform.position, LeftPoint.transform.rotation);

            anime.SetTrigger("ShootLeft");
            particle.GetComponent<Rigidbody>().linearVelocity = LeftPoint.transform.forward * ParticleSpeed;
            Destroy(particle, 10);
            ShootInt++;




        }
        else if(ShootInt == 1)
        {
            GameObject particle2 = Instantiate(Prefab_Particle, RightPoint.transform.position, RightPoint.transform.rotation);

            particle2.GetComponent<Rigidbody>().linearVelocity = RightPoint.transform.forward * ParticleSpeed;
            anime.SetTrigger("ShootRight");
            Destroy(particle2, 10);
            ShootInt--;
        }
       



    }
}
