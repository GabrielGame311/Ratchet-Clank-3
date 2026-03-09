using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipFreefall : MonoBehaviour
{
    public GameObject player;
    public GameObject playerholder;
    public GameObject[] robots;
    public Transform doorPosition;
    public float speed = 5f;
    public Transform SpawnPoint;

    [Header("Fall Settings")]
    public float fallSpeed = 15f;
    public float slowSpeed = 2f;
    public float slowDownDistance = 3f;
    public LayerMask groundLayer;
    public float slowDownDuration = 1.5f;
    Animator anime;
    CharacterController controller;
    RatchetController playercontroller;

    void Start()
    {
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        // Ignorera kollisioner mellan robotar direkt
        for (int i = 0; i < robots.Length; i++)
        {
            for (int j = i + 1; j < robots.Length; j++)
            {
                Physics.IgnoreCollision(robots[i].GetComponent<Collider>(), robots[j].GetComponent<Collider>());
            }
        }

        // Ställ in startposition
        playercontroller = player.GetComponent<RatchetController>();
        playercontroller.CanMove = false;

        controller = player.GetComponentInChildren<CharacterController>();
        controller.enabled = false;
        
        //player.transform.position = SpawnPoint.position;
       // player.transform.rotation = SpawnPoint.rotation;
        playerholder.transform.position = SpawnPoint.position;
        playerholder.transform.rotation = SpawnPoint.rotation;
        controller.enabled = true;

        StartCoroutine(MoveRobotsThenPlayer());
    }

    IEnumerator MoveRobotsThenPlayer()
    {
        foreach (var robot in robots)
        {
            StartCoroutine(MoveToDoorAndJump(robot, true));
            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(1.0f);

        
        StartCoroutine(MoveToDoorAndJump(player, false));
    }

    IEnumerator MoveToDoorAndJump(GameObject character, bool isRobot)
    {
        // 1. Gå fram till dörren
        // 1. Gå fram till dörren
        while (Vector3.Distance(new Vector3(0, 0, character.transform.position.z), new Vector3(0, 0, doorPosition.position.z)) > 0.3f)
        {
            // Vi skapar en stabil framåtrörelse med lite press nedåt (gravity)
            Vector3 direction = character.transform.forward * speed;
            direction.y = -9.81f; // Pressa ner gubben mot golvet så han inte "lyfter"

            if (!isRobot)
            {
                anime.SetBool("Run", true);

                
            }
            else
            {
                // Hämta robotens script och sätt animationen
                var ranger = character.GetComponent<GalacticRangers>();
                ranger.RangersModeActive = false;
                ranger.HeadAnime.SetBool("Run", true);
                ranger.FootAnime.SetBool("Run", true);
            }
           
           


            CharacterController cc = character.GetComponentInChildren<CharacterController>();
            if (cc != null)
            {
                cc.Move(direction * Time.deltaTime);
               
            }
            else
            {
                character.transform.Translate(Vector3.forward * speed * Time.deltaTime);
                
            }
           
            yield return null;
        }

        if (!isRobot)
        {
            anime.SetBool("Run", false);
            
        }
       

        // 2. Själva hoppet ut från kanten
        float jumpForce = 5f; // Hur långt fram de hoppar
        float jumpUp = 2f;    // En liten båge uppåt

        // Vi skapar en hopp-rörelse
        Vector3 jumpVelocity = (character.transform.forward * jumpForce) + (Vector3.up * jumpUp);

        if (isRobot)
        {
            // För robotar: Starta fallet men skicka med hopp-kraften
            StartCoroutine(IndividualRobotFall(character, jumpVelocity));
        }
        else
        {
            freefall ff = character.GetComponent<freefall>();
            CharacterController cc = character.GetComponentInChildren<CharacterController>();
            anime.SetBool("Run", false);
            
            anime.SetTrigger("Jump");

            // Ge hoppet en chans att synas innan freefall tar över helt
            StartCoroutine(PlayerJumpMomentum(cc, jumpVelocity));

            if (ff != null)
            {
                // Om ditt freefall-skript har en funktion som nollställer fart, 
                // kalla på den EFTER hoppet eller modifiera den.
                ff.RunForward();
                ff.ItsFalling = true;
                // Vi väntar ett litet ögonblick med att säga "nu faller vi fritt" 
                // så att momentum-skriptet hinner knuffa ut oss från skeppet
                StartCoroutine(DelayedFreefall(ff, 0.2f));
            }
           
            //playercontroller.CanMove = true;
           
        }
    }
    IEnumerator DelayedFreefall(freefall ff, float delay)
    {
        yield return new WaitForSeconds(delay);
        ff.ItsFalling = true;
    }

    IEnumerator PlayerJumpMomentum(CharacterController cc, Vector3 velocity)
    {
        float timer = 0;
        float duration = 0.8f; // Hur länge "hoppet" framåt ska påverka spelaren

        while (timer < duration)
        {
            // Lägg till gravitation på hopp-hastigheten
            velocity.y += -8 * Time.deltaTime;

            // Flytta spelaren med CharacterController
            if (cc != null && cc.enabled)
            {
                cc.Move(velocity * Time.deltaTime);
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

   

    IEnumerator IndividualRobotFall(GameObject robot, Vector3 currentVelocity)
    {
        // Stäng av AI så den inte försöker gå mitt i luften
        
        var ranger = robot.GetComponent<GalacticRangers>();
       
        float gravity = -9.81f;
        bool falling = true;
        // Vi stänger av Run-animationen precis när de hoppar ut
        ranger.HeadAnime.SetBool("Run", false);
        ranger.FootAnime.SetBool("Run", false);
        ranger.HeadAnime.SetBool("FreeFall", true);
        ranger.FootAnime.SetBool("FreeFall", true);
        // 1. Lägg till gravitation på Y-axeln över tid
        currentVelocity.y -= gravity * Time.deltaTime;

        // 2. Flytta roboten baserat på hastigheten (framåt + neråt)
        robot.transform.position += currentVelocity * Time.deltaTime;


        while (falling)
        {
            
            // 3. Håll roboten innanför den gröna boxen (Vägg-känsla)
            Vector3 pos = robot.transform.position;
            
            robot.transform.position = pos;

            // 4. Mark-check (Raycast)
            RaycastHit hit;
            // Vi kollar lite framåt i rörelseriktningen
            Vector3 rayOrigin = robot.transform.position + (new Vector3(currentVelocity.x, 0, currentVelocity.z).normalized * 0.5f);

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, slowDownDistance, groundLayer))
            {
                falling = false;
                StartCoroutine(SlowDownAndStop(robot, hit.point.y));
            }

            yield return null;
        }
    }

    IEnumerator SlowDownAndStop(GameObject robot, float groundHeight)
    {
        float elapsedTime = 0f;
        Vector3 startPos = robot.transform.position;
        Vector3 endPos = new Vector3(startPos.x, groundHeight, startPos.z);

        while (elapsedTime < slowDownDuration)
        {
            robot.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / slowDownDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        var ranger = robot.GetComponent<GalacticRangers>();
        ranger.RangersModeActive = true;
        ranger.HeadAnime.SetBool("FreeFall", false);
        ranger.FootAnime.SetBool("FreeFall", false);
        robot.transform.position = endPos;

        // Tvinga ner dem och lås dem till marken
        Rigidbody rb = robot.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        robot.GetComponent<GalacticRangers>().enabled = true;
    }
}