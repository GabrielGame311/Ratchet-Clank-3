using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class IOSController : MonoBehaviour
{

    RatchetController RatchetController_;
    public float RotationSpeed;
    public Joystick JoyStick_;
    public Joystick JoyStick_Right;
    
    public int SelectWeapon;
    public GameObject QuickSelect_;

    public Slider Slider_;

    public static IOSController IosController_;
    TorretGun TorretGun_;
    Shoutgun ShoutGun_;
    GranadeShooter GranadeShooter_;
    GunBall GunBall_;
    GunShooter GunShooter_;
    public GameObject EnterRangerShip_;
    public float RotationSpeedY;
    public Button FireButton_;

    public GameObject RightIos_;

    public GameObject PlayerUI_;
    public GameObject WeapoinUI_;

    // Start is called before the first frame update
    void Start()
    {
       RatchetController_ = GameObject.FindObjectOfType<RatchetController>();

        IosController_ = GetComponent<IOSController>();





    }

    private void FixedUpdate()
    {
        if (PlayerShip.playership_.gameObject != null)
        {
            PlayerUI_.SetActive(false);
            WeapoinUI_.SetActive(false);
        }
        else
        {
            PlayerUI_.SetActive(true);
            WeapoinUI_.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = JoyStick_Right.Horizontal;
        float verticalInput = JoyStick_Right.Vertical;

        TorretGun_ = GameObject.FindObjectOfType<TorretGun>();
        ShoutGun_ = GameObject.FindObjectOfType<Shoutgun>();
        GranadeShooter_ = GameObject.FindObjectOfType<GranadeShooter>();
        GunBall_ = GameObject.FindObjectOfType<GunBall>();
        GunShooter_ = GameObject.FindObjectOfType<GunShooter>();




        if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
        {
            RatchetController_.cine.m_XAxis.Value = horizontalInput * RotationSpeed;
            RatchetController_.cine.m_YAxis.Value = verticalInput * RotationSpeedY;
        }
    }

    public void Jump()
    {
        RatchetController_.RatchetJump();
    }

    public void Scrow()
    {
        BridegScrow.BridgeScrow_.Scrowing = true;
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public void ShipFire()
    {
        PlayerShip.playership_.ShootingIos();
    }

    public void OnPointerDown()
    {
        // The button is pressed
        PlayerShip.playership_.OnPointerDown();
    }

    public void OnPointerUp()
    {
        PlayerShip.playership_.OnPointerUp();
    }

   

    public void Fire()
    {
         
        if(TorretGun_ != null)
        {
            TorretGun_.Fire();
        }
        if (ShoutGun_ != null)
        {
            ShoutGun_.Shoots();
        }
        if (GranadeShooter_ != null)
        {
            GranadeShooter_.ThrowBall();
        }
        if (GunBall_ != null)
        {
            GunBall_.ThrowBall();
        }
        if(GunShooter_ != null)
        {
            GunShooter_.shoot();
        }
 


    }

    public void QuickSelect()
    {
        if(SelectWeapon == 0)
        {
            QuickSelect_.SetActive(true);
            Time.timeScale = 0;
            SelectWeapon++;
        }
        else if(SelectWeapon == 1)
        {
            QuickSelect_.SetActive(false);
            Time.timeScale = 1;
            SelectWeapon--;
        }
    }
}
