using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GranadeShooter : MonoBehaviour
{
    public GameObject GranadeBall;
    public Transform Spawn;
    public float ForceBall = 360;
    public AudioSource sound;
    public AudioClip ThrowSound;
    public bool shoot = false;
    PlayerControlls controlls;
    public Transform Sight_;
    // Start is called before the first frame update
    void Start()
    {
        sound = GameObject.FindGameObjectWithTag("Bolt").GetComponent<AudioSource>();

       
    }

    private void Awake()
    {
        controlls = new PlayerControlls();
    }

    private void OnEnable()
    {
        controlls.PlaystationControlls.Shoot.performed += Fire;
        controlls.Enable();
    }

    private void OnDisable()
    {
        controlls.PlaystationControlls.Shoot.performed -= Fire;
        controlls.Disable();
    }


    // Update is called once per frame
    void Update()
    {

        if (GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo > 0)
        {
           
                shoot = true;
            

            
        }
        else
        {
            shoot = false;
        }

        if (GetComponent<WeaponAmmos>().enabled)
        {
            if (IOSController.IosController_ == null)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    ThrowBall();
                }
            }
        }


         

        


    }

    public void Fire(InputAction.CallbackContext context)
    {
        ThrowBall();
    }

    public void ThrowBall()
    {
        if(shoot == true)
        {
            GameObject ball = Instantiate(GranadeBall, Spawn.transform.position, Spawn.transform.rotation);
            //ball.GetComponent<Rigidbody>().AddForce(Spawn.transform.forward * ForceBall);
            Vector3 direction = (Sight_.position - Spawn.position).normalized;

            // Skjut granaten i en båge genom att lägga till en uppåtkomponent
            Vector3 launchVelocity = direction * ForceBall + Vector3.up * (ForceBall / 2f);
            ball.GetComponent<Rigidbody>().velocity = launchVelocity;

            // Aktivera gravitation så att den faller naturligt
            ball.GetComponent<Rigidbody>().useGravity = true;


            sound.PlayOneShot(ThrowSound);
            GameObject.FindObjectOfType<WeaponAmmos>().GetComponent<WeaponAmmos>().Ammo -= 1;
            Destroy(ball, 2);

            if (GetComponent<WeaponAmmos>().Ammo < GetComponent<WeaponAmmos>().MaxAmmo)
            {
                GameObject.FindObjectOfType<VendingMenu>().Price += GetComponent<WeaponAmmos>().havePrice;
            }
            shoot = false;
        }

       



    }

    IEnumerator wait()
    {

        shoot = false;
        yield return new WaitForSeconds(3);

        shoot = true;
    }
}
