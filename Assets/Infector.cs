using UnityEngine;
using System.Collections;



public class Infector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject sludgePrefab;
    public Transform shootPoint;
    public float BallSpeed;
    public bool shoot = true;
    private WeaponAmmos weaponAmmo;

    // Start is called before the first frame update
    void Start()
    {
        weaponAmmo = GetComponent<WeaponAmmos>();
    }

    // Update is called once per frame
    void Update()
    {

        if (weaponAmmo != null && weaponAmmo.Ammo > 0)
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



    public void ThrowBall()
    {
        if (shoot == true && weaponAmmo != null && weaponAmmo.TryShoot())
        {
            GameObject ball = Instantiate(sludgePrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            ballRb.AddForce((shootPoint.forward + Vector3.up * 0.5f).normalized * 15f, ForceMode.Impulse);
            StartCoroutine(wait());
        }
    }

    IEnumerator wait()
    {
        shoot = false;
        yield return new WaitForSeconds(3);

        shoot = true;
    }
}
