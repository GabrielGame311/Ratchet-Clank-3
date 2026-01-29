using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipFreefall : MonoBehaviour
{
    public GameObject player;
    public GameObject playerholder;// Spelaren
    public GameObject[] robots; // Robotarna
    public Transform doorPosition; // Dörrens position
    public float speed = 5f; // Rörelsehastighet
    public Transform SpawnPoint;

    //RobotsFalling
    public float fallSpeed = 10f; // Hastighet när robotarna flyger neråt
    public float slowSpeed = 2f; // Hastighet när robotarna saktar ner
    public float slowDownDistance = 2f; // Avstånd från marken när de börjar sakta ner
    public LayerMask groundLayer; // Layer-mask för att identifiera marken
    public float slowDownDuration = 2f; // Tid de saktar ner innan de stannar

    private bool isSlowingDown = false; 
    void Start()
    {

      //  player = GameObject.FindGameObjectWithTag("Player");

        // Starta sekvensen där robotarna rör sig först, följt av spelaren
        player.GetComponentInChildren<CharacterController>().enabled = false;
        playerholder.transform.rotation = SpawnPoint.rotation;

        playerholder.transform.position = SpawnPoint.transform.position;

        
        StartCoroutine(MoveRobotsThenPlayer());
    }

    IEnumerator MoveRobotsThenPlayer()
    {
        // Förflytta robotarna mot dörren
        foreach (var robot in robots)
        {
            StartCoroutine(MoveToDoor(robot));
        }

        // Vänta 2 sekunder innan spelaren börjar röra sig
        yield return new WaitForSeconds(2f);

        // Förflytta spelaren mot dörren
        StartCoroutine(MoveToDoor(player));
    }

    IEnumerator MoveToDoor(GameObject character)
    {
        // Flytta karaktären rakt fram tills den är nära dörrens z-position
        while (Mathf.Abs(character.transform.position.z - doorPosition.position.z) > 0.1f)
        {
            // Flytta karaktären rakt framåt längs dess lokala z-axel
            character.transform.Translate(Vector3.forward * speed * Time.deltaTime);

            yield return null;
        }

       

        // När karaktären når dörren, kan du exempelvis starta hoppet eller någon annan handling
        Jump();

        StartCoroutine(PlayerJump(player));
    }


    IEnumerator PlayerJump(GameObject character)
    {
        yield return new WaitForSeconds(3);
        character.GetComponent<CharacterController>().enabled = true;
        character.GetComponent<freefall>().ItsFalling = true;
       
    }

    void Jump()
    {

        player.GetComponent<freefall>().RunForward();


            foreach (var robot in robots)
            {
                robot.GetComponent<Rigidbody>().useGravity = false;
                GalacticRangers gl = robot.GetComponent<GalacticRangers>();
                gl.enabled = false;
                isSlowingDown = true;
            }
        




    }

    private void Update()
    {
        foreach (var robot in robots)
        {
            if (isSlowingDown)
            {
                // Flytta roboten neråt
                robot.transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

                // Använd Raycast för att kontrollera avståndet till marken
                RaycastHit hit;
                if (Physics.Raycast(robot.transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
                {
                    float distanceToGround = hit.distance;

                    // Kontrollera om roboten är inom nedtrappningsavståndet
                    if (distanceToGround <= slowDownDistance)
                    {
                        StartCoroutine(SlowDownAndStop(robot, hit.point.y));
                    }
                }
            }
        }

        for (int i = 0; i < robots.Length; i++)
        {
            for (int j = i + 1; j < robots.Length; j++)
            {
                Collider collider1 = robots[i].GetComponent<Collider>();
                Collider collider2 = robots[j].GetComponent<Collider>();

                if (collider1 != null && collider2 != null)
                {
                    Physics.IgnoreCollision(collider1, collider2);
                }
            }
        }
    }


    private IEnumerator SlowDownAndStop(GameObject robot, float groundHeight)
    {
        isSlowingDown = false;

        // Sakta ner robotens fall under en viss tid
        float elapsedTime = 0f;
        while (elapsedTime < slowDownDuration)
        {
            robot.transform.Translate(Vector3.down * slowSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        GalacticRangers gl = robot.GetComponent<GalacticRangers>();
        gl.enabled = true;
        // Stoppa roboten och placera den exakt på marken
        
        robot.GetComponent<Rigidbody>().useGravity = true; // Detta stoppar all rörelse
        robot.transform.position = new Vector3(robot.transform.position.x, groundHeight, robot.transform.position.z);
        enabled = false;
        
    }
}
