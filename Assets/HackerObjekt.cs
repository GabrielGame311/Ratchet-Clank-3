using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HackerObjekt : MonoBehaviour
{
    // Start is called before the first frame update



    [FormerlySerializedAs("\u00E4rHackerSpel")]
    public bool isHackerGame = false;
    public Transform PunktAttCirklaRunt; // Detta �r din PointGo
    private GameObject Spelare; // Ratchet i det h�r fallet
    public  float cirkelRadie = 3f; // Hur stor cirkeln ska vara
    public float cirkelHastighet = 2f; // Hur snabbt spelaren cirklar
    [FormerlySerializedAs("r\u00F6relseHastighet")]
    public float movementSpeed = 5f; // Hastighet for att rora sig mot punkten
    public float rotationSpeed = 5f;
    public GameObject Timeline_;
    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = Quaternion.Euler(0, -155, 0);
        Spelare = GameObject.FindGameObjectWithTag("Player"); // Hittar Ratchet
    }

    void Update()
    {
        if (isHackerGame)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(StartaHackerSekvens());
            }
        }
    }

    IEnumerator StartaHackerSekvens()
    {
        // Aktivera hacker-verktyget
        WeaponSwitcher weaponSwitcher = FindObjectOfType<WeaponSwitcher>();
        if (weaponSwitcher != null) weaponSwitcher.HackerItemEnable();

        // St�nger av spelarens normala r�relse
        if (Spelare != null)
        {
            Spelare.GetComponent<CharacterController>().enabled = false;
        }

        RatchetController ratchetController = FindObjectOfType<RatchetController>();
        if (ratchetController != null) ratchetController.enabled = false;

        Player ratchet = FindObjectOfType<Player>();
        if (ratchet != null) ratchet.anime.SetBool("Run", true);

        // Cirkla runt punkten
        float vinkel = 0f;
        float cirkelTid = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < cirkelTid)
        {
            elapsedTime += Time.deltaTime;
            vinkel += (Time.deltaTime / cirkelTid) * Mathf.PI * 2; // J�mnare rotation

            // Ber�kna position i cirkeln
            float x = PunktAttCirklaRunt.position.x + Mathf.Cos(vinkel) * cirkelRadie;
            float z = PunktAttCirklaRunt.position.z + Mathf.Sin(vinkel) * cirkelRadie;

            Vector3 targetPosition = new Vector3(x, Spelare.transform.position.y, z);
            Spelare.transform.position = Vector3.MoveTowards(
                Spelare.transform.position,
                targetPosition,
                movementSpeed * Time.deltaTime
            );

            // Smidigare rotation mot mitten
            Quaternion targetRotation = Quaternion.LookRotation(PunktAttCirklaRunt.position - Spelare.transform.position);
            Spelare.transform.rotation = Quaternion.RotateTowards(Spelare.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            ratchet.anime.SetBool("Run", true);
            yield return null;
        }

        // R�r sig mot punkten med mjuk rotation
        while (Vector3.Distance(Spelare.transform.position, PunktAttCirklaRunt.position) > 0.1f)
        {
            Spelare.transform.position = Vector3.MoveTowards(
                Spelare.transform.position,
                PunktAttCirklaRunt.position,
                movementSpeed * Time.deltaTime
            );

            Quaternion targetRotation = Quaternion.LookRotation(PunktAttCirklaRunt.position - Spelare.transform.position);
            Spelare.transform.rotation = Quaternion.RotateTowards(Spelare.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            yield return null;
        }

        // Exakt position
        Spelare.transform.position = PunktAttCirklaRunt.position;

        // **Mjuk rotation till (0,0,0)**
        // Mjukare rotation till slutl�ge under en l�ngre tid
        float rotationTid = 1.5f; // Hur l�ng tid rotationen tar
        float tid = 0f;
        Quaternion startRotation = Spelare.transform.rotation;
        Quaternion slutRotation = Quaternion.Euler(0, -155, 0);

        while (tid < rotationTid)
        {
            tid += Time.deltaTime;
            float t = tid / rotationTid;
            Spelare.transform.rotation = Quaternion.Slerp(startRotation, slutRotation, t);
            yield return null;
        }


        // Animationer efter ankomst
        if (ratchet != null)
        {
            ratchet.anime.SetBool("Put", true);
            Timeline_.SetActive(true);
            ratchet.anime.SetBool("Run", false);
        }

        yield return new WaitForSeconds(2);

        // Aktivera hacker-spelet
        HackerGameEnable hackerGame = FindObjectOfType<HackerGameEnable>();
        Timeline_.SetActive(false);
        if (hackerGame != null) hackerGame.EnableHackerGame();

        if (Spelare != null)
        {
            Spelare.GetComponent<CharacterController>().enabled = true;
        }

       
        if (ratchetController != null) ratchetController.enabled = true;
    }



    private void OnTriggerEnter(Collider other)
    {
       

        if(other.CompareTag("Player"))
        {
             isHackerGame = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {

         if(other.CompareTag("Player"))
        {
            
            isHackerGame = false;
        }
    }
}
