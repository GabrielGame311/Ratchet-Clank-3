using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using System.IO;


public class GadgeTronArmor : MonoBehaviour
{
    public PlayableDirector ArmorAnime;
    public static GadgeTronArmor Instance;
    public Camera cam;
    private Animator anime;
    private bool isTrigger = false;
    public bool isGadgeTron = false;
    private GameObject player;
    private CharacterController playerController;
    private PlayerMovement playerMovement;

    private AudioSource sound;
    public AudioClip soundFx;

    [Header("Movement to Point")]
    public float moveSpeedToPoint = 5.0f; // Speed to move to the armor point
    public float arrivalThreshold = 0.1f; // Distance threshold to consider "arrived"
    public float rotationSpeed = 10.0f;   // Speed of rotation toward target
    Animator anime2;
    public Transform point;
    public AllGameData data;
    public int IntCount;

    void Start()
    {
        data = GameObject.FindGameObjectWithTag("Playerholder").GetComponent<AllGameData>();
        Instance = this;
        sound = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        anime = GetComponent<Animator>();
        anime2 = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        playerController = player.GetComponent<CharacterController>();
        playerMovement = player.GetComponent<PlayerMovement>();

        if (!playerController) Debug.LogError("Player has no CharacterController!");
        if (!playerMovement) Debug.LogError("Player has no PlayerMovement script!");
    }

    void Update()
    {
        if (isTrigger)
        {


            if (!isGadgeTron)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    StartCoroutine(ActivateGadgeTron());
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    GadgeTronExit();
                }

            }
               
                        
                    

                    


                        
                        if(GameObject.FindObjectOfType<GadgeTronArmorUI>().ArmorBoltPrice.Length > IntCount)
                        {

                            if (!isGadgeTron)
                            {
                                
                            }
                            else
                            {
                               
                                if (Input.GetKeyDown(KeyCode.Space))
                                {
                                    GameObject.FindObjectOfType<Bolts>().bolt -= GameObject.FindObjectOfType<GadgeTronArmorUI>().ArmorBoltPrice[IntCount];
                                    IntCount++;
                                    
                                    ChangeArmor();
                                }
                            }
                        }

                    
                
            

           
        }
    }

    private IEnumerator ActivateGadgeTron()
    {
        // Disable player control
        AllGameData.Instance.DisablePlayerDo();
        // For example, after calling ChangeArmor or when exiting the game
       

        if (playerMovement != null) playerMovement.enabled = false;

        // Target position is this armor object's position
        point.transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
        yield return new WaitForSeconds(0);
        // Start "Run" animation
        anime2.SetBool("Run", true);

        // Move player to the target position while rotating to face it
        while (Vector3.Distance(player.transform.position, point.transform.position) > arrivalThreshold)
        {
            // Calculate direction to target
            Vector3 direction = (point.transform.position - player.transform.position).normalized;

            // Rotate player to face the target
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                
            }

            // Move player toward the target
            playerController.Move(direction * moveSpeedToPoint * Time.deltaTime);

            yield return null; // Wait for next frame
        }

        // Ensure player is exactly at the point
         
         player.transform.position = point.transform.position;
        
        // Stop "Run" animation before activating "Gadgetron"
        anime2.SetBool("Run", false);
        yield return new WaitForSeconds(0.2f); // Small pause
        sound.PlayOneShot(soundFx);

        anime.SetBool("Gadgetron", true);
        isGadgeTron = true;
        
    }


    public void ActiveGadgetronArmor()
    {
        
        GameObject.FindObjectOfType<ArmorUI>().ActiveArmorUI();
    }

    public void DisableGadgetronArmor()
    {
        GameObject.FindObjectOfType<ArmorUI>().DisableArmorUI();
    }


    public void GadgeTron()
    {
        sound.PlayOneShot(soundFx);
        anime.SetBool("Gadgetron", true);
        AllGameData.Instance.DisablePlayerDo();
        isGadgeTron = true;
    }

    public void GadgeTronExit()
    {
        
        AllGameData.Instance.EnablePlayerDo();
        anime.SetBool("Gadgetron", false);
        sound.PlayOneShot(soundFx);

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        isGadgeTron = false;
        
    }

    public void ChangeArmor()
    {
        GameObject.FindObjectOfType<GadgeTronArmorUI>().Armor++;
        AllGameData.Instance.EnablePlayerDo();
        ArmorAnime.Play();
        AllGameData.Instance.Armor++;
        anime.SetBool("Gadgetron", false);

        isGadgeTron = false;
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        DisableGadgetronArmor();

        SaveSystem.SaveGame(AllGameData.Instance.CurrentSaveSlot);
            
        
               
        //cam.enabled = true;
       
    }

    public void CancelAnime()
    {
        AllGameData.Instance.EnablePlayerDo();
       // cam.enabled = false;
        if (playerMovement != null) playerMovement.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTrigger = false;
        }
    }
}