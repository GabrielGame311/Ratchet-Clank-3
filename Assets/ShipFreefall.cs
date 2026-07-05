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
    public AudioSource Sound;
    void Start()
    {
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerholder = player;
        player.GetComponent<RatchetController>().cine.m_XAxis.Value = 95;
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

        playerholder.transform.position = SpawnPoint.position;
        playerholder.transform.rotation = SpawnPoint.rotation;
        controller.enabled = true;

        // Starta sekvensen
        StartCoroutine(MoveRobotsThenPlayer());
    }

    IEnumerator MoveRobotsThenPlayer()
    {
        yield return new WaitForSeconds(2);

        Sound.Play();

        yield return new WaitForSeconds(0);

        // Starta spelaren
        StartCoroutine(MoveToDoorAndJump(player, false, -1));

        // Starta ALLA robotars rörelse
        for (int i = 0; i < robots.Length; i++)
        {
            StartCoroutine(MoveToDoorAndJump(robots[i], true, i));
        }

        yield return null;
    }

    IEnumerator MoveToDoorAndJump(GameObject character, bool isRobot, int robotIndex)
    {
        while (Vector3.Distance(new Vector3(0, 0, player.transform.position.z), new Vector3(0, 0, doorPosition.position.z)) > 0.3f)
        {
            Vector3 direction = character.transform.forward * speed;
            direction.y = -9.81f;

            if (!isRobot)
            {
                anime.SetBool("Run", true);
            }
            else
            {
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

        float jumpForce = 5f;
        float jumpUp = 2f;

        Vector3 jumpVelocity = (character.transform.forward * jumpForce) + (Vector3.up * jumpUp);

        if (isRobot)
        {
            StartCoroutine(IndividualRobotFall(character, jumpVelocity, robotIndex));
        }
        else
        {
            freefall ff = character.GetComponent<freefall>();
            CharacterController cc = character.GetComponentInChildren<CharacterController>();
            anime.SetBool("Run", false);
            anime.SetTrigger("Jump");

            StartCoroutine(PlayerJumpMomentum(cc, jumpVelocity));

            if (ff != null)
            {
                ff.RunForward();
                StartCoroutine(DelayedFreefall(ff, 0.2f));
            }
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
        float duration = 0.8f;

        while (timer < duration)
        {
            velocity.y += -8 * Time.deltaTime;

            if (cc != null && cc.enabled)
            {
                cc.Move(velocity * Time.deltaTime);
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator IndividualRobotFall(GameObject robot, Vector3 currentVelocity, int robotIndex)
    {
        var ranger = robot.GetComponent<GalacticRangers>();
        bool falling = true;

        ranger.HeadAnime.SetBool("Run", false);
        ranger.FootAnime.SetBool("Run", false);
        ranger.HeadAnime.SetBool("FreeFall", true);
        ranger.FootAnime.SetBool("FreeFall", true);

        CharacterController cc = robot.GetComponentInChildren<CharacterController>();

        // Beräkna fasta formationsplatser (Offsets) runt spelaren
        float sideSign = (robotIndex % 2 == 0) ? 1f : -1f;
        float multiplier = (robotIndex / 2) + 1;

        // Formationsavstånd: t.ex. 2.5 enheter åt sidan, 2.0 enheter bakom spelaren
        Vector3 formationOffset = new Vector3(sideSign * multiplier * 2.5f, 0f, -multiplier * 2.0f);

        // Mjuk övergång i början från hoppet till formationen
        float formationLerpTime = 0f;

        while (falling)
        {
            formationLerpTime += Time.deltaTime;

            // Hitta den exakta globala målpositionen baserat på var spelaren befinner sig just nu
            Vector3 targetWorldPos = player.transform.position + player.transform.TransformDirection(formationOffset);

            // Beräkna hur mycket roboten måste röra sig horisontellt (X och Z) för att hålla sin plats i formationen
            Vector3 nextHorizontalPos = Vector3.Lerp(robot.transform.position, targetWorldPos, formationLerpTime * 2f);

            // Hantera rörelsen via CharacterController eller direkt transform
            Vector3 moveDirection = new Vector3(nextHorizontalPos.x - robot.transform.position.x, -fallSpeed * Time.deltaTime, nextHorizontalPos.z - robot.transform.position.z);

            if (cc != null && cc.enabled)
            {
                // Om vi använder cc.Move skickar vi med den beräknade rörelsen per bildruta
                cc.Move(new Vector3(moveDirection.x, moveDirection.y, moveDirection.z));
            }
            else
            {
                robot.transform.position = new Vector3(nextHorizontalPos.x, robot.transform.position.y - (fallSpeed * Time.deltaTime), nextHorizontalPos.z);
            }

            // Raycast kollar marken under robotens koordinater
            RaycastHit hit;
            Vector3 rayOrigin = robot.transform.position;

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, slowDownDistance, groundLayer))
            {
                falling = false;
                if (cc != null) cc.enabled = false;

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

        var ranger = robot.GetComponent<GalacticRangers>();
        if (ranger.GetComponent<CharacterController>().isGrounded)
        {

            ranger.RangersModeActive = true;
            ranger.HeadAnime.SetBool("FreeFall", false);
            ranger.FootAnime.SetBool("FreeFall", false);
            robot.transform.position = endPos;
        }
        else
        {
            

            while (elapsedTime < slowDownDuration)
            {
                robot.transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / slowDownDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }


        CharacterController cc = robot.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = true;
       

        ranger.enabled = true;
    }
}