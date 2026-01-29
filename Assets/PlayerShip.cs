using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;


public class PlayerShip : MonoBehaviour
{

    public float MoveSpeed;
    public float ShootForce;
    public GameObject ParticlePrefab;


    public int CurrentHealth = 100;
    public int MaxHealth;
    public string LoadScene;
    public Transform point;
    public CinemachineVirtualCamera cn;
    public float mouseSensitivity = 0;
    float yRotation = 0;
    float xRotation = 0;
    float MovmentV = 0;
    public float ShootTime;
    float startshoot;
    public Transform Ship;

    public static PlayerShip playership_;

    //IOS
    public bool IsTrigger = false;

    public bool IsController = false;

    private bool isHolding = false;
    private float holdTimer = 0.0f;
    public int soundplaying;
    public AudioClip[] soundFX;
    public AudioSource sound;
    public float holdTimeThreshold = 1.0f;
    GameObject player;
    GameObject playerHolder;

    float startshoots;
    bool isshooting = false;
    float shooter = 2;
    // Start is called before the first frame update
    void Start()
    {
        startshoots = shooter;
        sound.GetComponent<AudioSource>();
        playerHolder = GameObject.FindGameObjectWithTag("Playerholder");
        player = GameObject.FindObjectOfType<RatchetController>().gameObject;

       // player.SetActive(false);
        playership_ = GetComponent<PlayerShip>();
        MaxHealth = CurrentHealth;
        startshoot = ShootTime;
        cn = GetComponentInChildren<CinemachineVirtualCamera>();

       
    }

    // Update is called once per frame
    void Update()
    {

        

        

        if (IsController)
        {
            HealthBar.HealthBar_.SetHealth(CurrentHealth);
            HealthBar.HealthBar_.HealthText_.text = CurrentHealth.ToString() + " / " + MaxHealth.ToString();



            if (IOSController.IosController_ != null)
            {
                float h = IOSController.IosController_.JoyStick_.Horizontal * MoveSpeed * Time.deltaTime;
                float v = IOSController.IosController_.JoyStick_.Vertical * MoveSpeed * Time.deltaTime;

                transform.Translate(h, 0, v);
                float mouseX = IOSController.IosController_.JoyStick_Right.Horizontal * mouseSensitivity * Time.deltaTime;
                float mouseY = IOSController.IosController_.JoyStick_Right.Vertical * mouseSensitivity * Time.deltaTime;


                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -30, 30);
                yRotation += mouseX;
                transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
            }
            else
            {
                float h = Input.GetAxisRaw("Horizontal") * MoveSpeed * Time.deltaTime;
                float v = Input.GetAxisRaw("Vertical") * MoveSpeed * Time.deltaTime;
                transform.Translate(h, 0, v);

                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;


                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -30, 30);
                yRotation += mouseX;

                transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);

                if(isshooting == true)
                {
                    shooter -= Time.deltaTime;


                    if(shooter < 0)
                    {

                        isshooting = false;
                        startshoots = shooter;
                    }

                }


                if(isshooting == false)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        Shoot();

                        isshooting = true;

                    }

                }







                if (Input.GetKey(KeyCode.Mouse0))
                {
                    OnPointerDown();
                }
                else
                {
                    OnPointerUp();
                }

            }



















            /// transform.Rotate(0, mouseX, 0);


            if (Input.GetKey(KeyCode.Space))
            {

                transform.position += Vector3.up * MoveSpeed * Time.deltaTime;

            }

            if (Input.GetKey(KeyCode.LeftShift))
            {


                transform.position += Vector3.down * MoveSpeed * Time.deltaTime;
            }







            if (isHolding)
            {
                holdTimer += Time.deltaTime;

                // Check if the hold time has reached the threshold
                if (holdTimer >= holdTimeThreshold)
                {

                    ShootTime -= Time.deltaTime;

                    if (ShootTime < 0)
                    {
                        ShootTime = startshoot;
                        Shoot();
                    }
                }
            }


           


           

        }

        if (IsTrigger)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                playerHolder.SetActive(false);
                cn.enabled = true;
                GetComponentInChildren<Camera>().enabled = true;
                IsController = true;

            }
        }

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

    public void ShootingIos()
    {
        ShootTime = startshoot;
        Shoot();

        
    }
    public void OnPointerDown()
    {
        // The button is pressed
        isHolding = true;
    }

    public void OnPointerUp()
    {
        // The button is released
        isHolding = false;
        holdTimer = 0.0f; // Reset the timer
    }


    public void TakeDamage(int damage)
    {

        CurrentHealth -= damage;



        if(CurrentHealth < 0)
        {
            CurrentHealth = 0;

            Die();
        }


    }


    void Die()
    {

       

        StartCoroutine(wait());


    }

    private void OnDestroy()
    {
        


    }

    IEnumerator wait()
    {


        if (LoadScene != null)
        {
            playerHolder.SetActive(true);
            player.GetComponent<CharacterController>().enabled = false;
            player.GetComponent<Rigidbody>().AddForce(Vector3.up * 200, ForceMode.Impulse);
            player.GetComponent<Rigidbody>().AddForce(Vector3.left * 200, ForceMode.Impulse);

            player.transform.position = transform.position;
            player.GetComponent<CharacterController>().enabled = true;

            cn.enabled = false;
            GetComponentInChildren<Camera>().enabled = false;
        }

        yield return new WaitForSeconds(1);
       

        yield return new WaitForSeconds(2);
        
           
        

        yield return new WaitForSeconds(2);
       
        Destroy(Ship);
        yield return new WaitForSeconds(2);

        if(LoadScene == null)
        {
            SceneManager.LoadScene(LoadScene);
        }
        else
        {
           
           
            Destroy(gameObject);
        }

    }


    public void Shoot()
    {

        if (soundplaying < soundFX.Length - 1)
        {
            soundplaying++;
        }
        else
        {
            soundplaying = 0;
        }

      



                        

                      

            sound.PlayOneShot(soundFX[soundplaying]);
         

        GameObject prefab = Instantiate(ParticlePrefab, point.transform.position, point.transform.rotation);
        prefab.GetComponent<Rigidbody>().velocity = point.transform.forward * ShootForce;

        Destroy(prefab, 10);

      

    }
}
