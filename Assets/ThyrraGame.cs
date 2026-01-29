using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


public class ThyrraGame : MonoBehaviour
{

    public Animator anime;

    bool IsTrigger = false;
    public GameObject Point_;
    public Transform PointEnemyButton;
    public float SpeedCamMove;
    public Camera cam;
    public CinemachineFreeLook freeLookCam;
    public CinemachineVirtualCamera dialogCam;
    bool PlayerMoving = false;
    public float RotationSpeed;
    public float PlayerMovingSpeed;
    public GameObject Player_;
    GameObject GameThyrraUI;
    bool IsMovingEnemy = false;
    public GameObject BlockScreen;
    // Start is called before the first frame update
    void Start()
    {
        GameThyrraUI = GameObject.FindObjectOfType<GameThyrra_UI>().GamePanel;
        Player_ = GameObject.FindGameObjectWithTag("Player");
        //Point_ = GameObject.FindGameObjectWithTag("DialogPoint");
       // anime = GetComponent<Animator>();
        freeLookCam = GameObject.FindObjectOfType<CinemachineFreeLook>();
        dialogCam = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsTrigger)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                anime.SetTrigger("Talk");

                IsTrigger = false;
               

               // StartDialog();
                StartCoroutine(WaitToDialog());
            }


        }

        if (PlayerMoving)
        {
            float dis = Vector3.Distance(Player_.transform.position, Point_.transform.position);

            if (dis > 0.1f)
            {
                Player_.GetComponent<RatchetController>().anime.SetBool("Walk", true);
                Vector3 direction = transform.position - Player_.transform.position;
                direction.y = 0f; // undvik att titta uppåt eller nedåt

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Player_.transform.rotation = Quaternion.RotateTowards(
                        Player_.transform.rotation,
                        targetRotation,
                        RotationSpeed* Time.deltaTime * 100f
                    );
                }

                Player_.GetComponent<CharacterController>().enabled = false;
                Player_.transform.position = Vector3.MoveTowards(
                    Player_.transform.position,
                    Point_.transform.position,
                    PlayerMovingSpeed * Time.deltaTime
                );
            }
            else
            {
                // Spelaren har nått punkten – stoppa rörelse & animation
                
                
                Player_.GetComponent<RatchetController>().anime.SetBool("Walk", false);
                PlayerMoving = false;

                // (valfritt) Starta nästa steg i dialog eller animation här
            }
        }


        if(IsMovingEnemy)
        {
            float dis = Vector3.Distance(transform.position, PointEnemyButton.transform.position);

            if (dis > 0.1f)
            {
                anime.SetBool("Walk", true);
                Vector3 direction = PointEnemyButton.transform.position - transform.position;
                direction.y = 0f; // undvik att titta uppåt eller nedåt

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        RotationSpeed * Time.deltaTime * 100f
                    );
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    PointEnemyButton.transform.position,
                    PlayerMovingSpeed * Time.deltaTime
                );
            }
            else
            {
                // Spelaren har nått punkten – stoppa rörelse & animation


                anime.SetBool("Walk", false);
                BlockScreen.GetComponent<Animator>().SetTrigger("Button");
                IsMovingEnemy = false;

                // (valfritt) Starta nästa steg i dialog eller animation här
            }

        }
    }

    IEnumerator WaitToDialog()
    {
        yield return new WaitForSeconds(0.3f);

        StartDialog();

        yield return new WaitForSeconds(1);
        dialogCam.transform.parent = null;
        PlayerMoving = true;

        yield return new WaitForSeconds(2);

        GameThyrraUI.SetActive(true);

    }

    public void StartDialog()
    {
        Player_.GetComponent<RatchetController>().enabled = false;
        dialogCam.enabled = true;
    }

    public void EndDialog()
    {
        dialogCam.transform.parent = Player_.transform;
        dialogCam.enabled = false;
        Player_.GetComponent<RatchetController>().enabled = true;
        Player_.GetComponent<CharacterController>().enabled = true;
        GetComponent<BoxCollider>().enabled = false;
        IsMovingEnemy = true;


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
