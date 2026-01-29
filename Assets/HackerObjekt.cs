using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackerObjekt : MonoBehaviour
{
    // Start is called before the first frame update



    public bool ÄrHackerSpel = false;
    public Transform PunktAttCirklaRunt; // Detta är din PointGo
    private GameObject Spelare; // Ratchet i det här fallet
    public  float cirkelRadie = 3f; // Hur stor cirkeln ska vara
    public float cirkelHastighet = 2f; // Hur snabbt spelaren cirklar
    public float rörelseHastighet = 5f; // Hastighet för att röra sig mot punkten
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
        if (ÄrHackerSpel)
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
        WeaponSwitcher weaponSwitcher = GameObject.FindObjectOfType<WeaponSwitcher>();
        if (weaponSwitcher != null) weaponSwitcher.HackerItemEnable();

        // Stänger av spelarens normala rörelse
        if (Spelare != null)
        {
            Spelare.GetComponent<CharacterController>().enabled = false;
        }

        RatchetController ratchetController = GameObject.FindObjectOfType<RatchetController>();
        if (ratchetController != null) ratchetController.enabled = false;

        Player ratchet = GameObject.FindObjectOfType<Player>();
        if (ratchet != null) ratchet.anime.SetBool("Run", true);

        // Cirkla runt punkten
        float vinkel = 0f;
        float cirkelTid = 2f;
        float förflutenTid = 0f;

        while (förflutenTid < cirkelTid)
        {
            förflutenTid += Time.deltaTime;
            vinkel += (Time.deltaTime / cirkelTid) * Mathf.PI * 2; // Jämnare rotation

            // Beräkna position i cirkeln
            float x = PunktAttCirklaRunt.position.x + Mathf.Cos(vinkel) * cirkelRadie;
            float z = PunktAttCirklaRunt.position.z + Mathf.Sin(vinkel) * cirkelRadie;

            Vector3 målPosition = new Vector3(x, Spelare.transform.position.y, z);
            Spelare.transform.position = Vector3.MoveTowards(
                Spelare.transform.position,
                målPosition,
                rörelseHastighet * Time.deltaTime
            );

            // Smidigare rotation mot mitten
            Quaternion targetRotation = Quaternion.LookRotation(PunktAttCirklaRunt.position - Spelare.transform.position);
            Spelare.transform.rotation = Quaternion.RotateTowards(Spelare.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            ratchet.anime.SetBool("Run", true);
            yield return null;
        }

        // Rör sig mot punkten med mjuk rotation
        while (Vector3.Distance(Spelare.transform.position, PunktAttCirklaRunt.position) > 0.1f)
        {
            Spelare.transform.position = Vector3.MoveTowards(
                Spelare.transform.position,
                PunktAttCirklaRunt.position,
                rörelseHastighet * Time.deltaTime
            );

            Quaternion targetRotation = Quaternion.LookRotation(PunktAttCirklaRunt.position - Spelare.transform.position);
            Spelare.transform.rotation = Quaternion.RotateTowards(Spelare.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            yield return null;
        }

        // Exakt position
        Spelare.transform.position = PunktAttCirklaRunt.position;

        // **Mjuk rotation till (0,0,0)**
        // Mjukare rotation till slutläge under en längre tid
        float rotationTid = 1.5f; // Hur lång tid rotationen tar
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
        HackerGameEnable hackerGame = GameObject.FindObjectOfType<HackerGameEnable>();
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
        ÄrHackerSpel = true;
    }

    private void OnTriggerExit(Collider other)
    {
        ÄrHackerSpel = false;
    }
}
