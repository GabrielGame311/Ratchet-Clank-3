using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class ShipFreefall : MonoBehaviour
{
    public GameObject player;
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

    [Header("Landing Targets (Dra in dina LandingPoints här)")]
    public Transform[] landingPoints;
    public bool WithoutPlayer = false;
    
    Animator anime;
    CharacterController controller;
    RatchetController playercontroller;
    public AudioSource Sound;

    void Start()
    {
        // 1. Hitta spelare och komponenter FÖRST av allt
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (player != null)
        {
            playercontroller = player.GetComponent<RatchetController>();
            if (playercontroller != null && playercontroller.cine != null)
            {
                playercontroller.cine.m_XAxis.Value = 95;
            }

            if (playercontroller != null) playercontroller.CanMove = false;

            controller = player.GetComponentInChildren<CharacterController>();
            if (controller != null && SpawnPoint != null)
            {
                controller.enabled = false;
                player.transform.position = SpawnPoint.transform.position;
                player.transform.rotation = SpawnPoint.transform.rotation;
                controller.enabled = true;
            }
        }

        GameObject ratchetObj = GameObject.FindGameObjectWithTag("Ratchet");
        if (ratchetObj != null)
        {
            anime = ratchetObj.GetComponent<Animator>();
        }

        // 2. Kontrollera om det finns giltiga robotar i listan
        bool hasValidRobots = false;
        if (robots != null && robots.Length > 0)
        {
            foreach (var r in robots)
            {
                if (r != null)
                {
                    hasValidRobots = true;
                    break;
                }
            }
        }

        // Ignorera kollisioner mellan robotar om de finns
        if (hasValidRobots)
        {
            for (int i = 0; i < robots.Length; i++)
            {
                for (int j = i + 1; j < robots.Length; j++)
                {
                    if (robots[i] != null && robots[j] != null)
                    {
                        Collider colI = robots[i].GetComponent<Collider>();
                        Collider colJ = robots[j].GetComponent<Collider>();
                        if (colI != null && colJ != null)
                        {
                            Physics.IgnoreCollision(colI, colJ);
                        }
                    }
                }

                if (robots[i] != null)
                {
                    var ranger = robots[i].GetComponent<GalacticRangers>();
                    if (ranger != null) ranger.IsFreefall = true;
                }
            }
        }

        // 3. Starta rätt sekvens
        if (WithoutPlayer)
        {
            if (player != null)
            {
                var ff = player.GetComponent<freefall>();
                if (ff != null) ff.ItsFalling = true;
            }
            if (hasValidRobots) StartCoroutine(MoveRobots());
        }
        else if (!hasValidRobots)
        {
            // BARA SPELAREN (Går till dörren ensam efter 2 sec)
            StartCoroutine(MovePlayerOnly());
        }
        else
        {
            // BÅDE SPELARE OCH ROBOTAR
            StartCoroutine(MoveRobotsThenPlayer());
        }
    }

    IEnumerator MovePlayerOnly()
    {
        yield return new WaitForSeconds(2f);

        if (Sound != null) Sound.Play();

        if (player != null)
        {
            StartCoroutine(MoveToDoorAndJump(player, false, -1));
        }
    }

    IEnumerator MoveRobots()
    {
        yield return new WaitForSeconds(2f);

        if (robots != null)
        {
            for (int i = 0; i < robots.Length; i++)
            {
                if (robots[i] != null)
                {
                    StartCoroutine(MoveToDoorAndJump(robots[i], true, i));
                }
            }
        }
    }

    IEnumerator MoveRobotsThenPlayer()
    {
        yield return new WaitForSeconds(2f);

        if (Sound != null) Sound.Play();

        if (robots != null)
        {
            for (int i = 0; i < robots.Length; i++)
            {
                if (robots[i] != null)
                {
                    StartCoroutine(MoveToDoorAndJump(robots[i], true, i));
                }
            }
        }

        yield return new WaitForSeconds(0.4f);

        if (player != null)
        {
            StartCoroutine(MoveToDoorAndJump(player, false, -1));
        }
    }

    IEnumerator MoveToDoorAndJump(GameObject character, bool isRobot, int robotIndex)
    {
        CharacterController cc = character.GetComponentInChildren<CharacterController>();

        float timer = 0f;
        float maxTime = 4f;

        if (cc != null && cc.enabled)
        {
            cc.Move(Vector3.zero);
        }

        while (timer < maxTime)
        {
            if (doorPosition == null) break;

            // Beräkna avstånd på golvplanet (X och Z)
            Vector3 currentPos = character.transform.position;
            Vector3 flatCurrent = new Vector3(currentPos.x, 0, currentPos.z);
            Vector3 flatDoor = new Vector3(doorPosition.position.x, 0, doorPosition.position.z);

            // Om karaktären är nära dörren (inom 0.5 meter), bryt slingans gång och hoppa
            if (Vector3.Distance(flatCurrent, flatDoor) <= 0.5f)
            {
                break;
            }

            timer += Time.deltaTime;

            // Riktning mot dörren
            Vector3 moveDirection = (flatDoor - flatCurrent).normalized;

            // Rotera karaktären mot dörren
            if (moveDirection != Vector3.zero)
            {
                character.transform.rotation = Quaternion.LookRotation(moveDirection);
            }

            // Sätt hastighet och gravitationskraft
            Vector3 velocity = moveDirection * speed;
            velocity.y = -9.81f;

            // Aktivera spring-animationer
            if (!isRobot)
            {
                if (anime != null) anime.SetBool("Run", true);
            }
            else
            {
                var ranger = character.GetComponent<GalacticRangers>();
                if (ranger != null)
                {
                    ranger.RangersModeActive = false;
                    if (ranger.HeadAnime != null) ranger.HeadAnime.SetBool("Run", true);
                    if (ranger.FootAnime != null) ranger.FootAnime.SetBool("Run", true);
                }
            }

            // Flytta karaktären
            if (cc != null && cc.enabled)
            {
                cc.Move(velocity * Time.deltaTime);
            }
            else
            {
                character.transform.position += velocity * Time.deltaTime;
            }

            yield return null;
        }

        if (timer >= maxTime)
        {
            Debug.LogWarning(character.name + " nådde tidsgränsen vid dörren.");
        }

        // Stäng av spring-animation
        if (!isRobot)
        {
            if (anime != null) anime.SetBool("Run", false);
        }
        else
        {
            var ranger = character.GetComponent<GalacticRangers>();
            if (ranger != null)
            {
                if (ranger.HeadAnime != null) ranger.HeadAnime.SetBool("Run", false);
                if (ranger.FootAnime != null) ranger.FootAnime.SetBool("Run", false);
            }
        }

        // Kraft för hoppet ut ur dörren
        float jumpForce = 5f;
        float jumpUp = 4f;
        Vector3 jumpVelocity = (character.transform.forward * jumpForce) + (Vector3.up * jumpUp);

        if (isRobot)
        {
            StartCoroutine(IndividualRobotFall(character, jumpVelocity, robotIndex));
        }
        else
        {
            freefall ff = character.GetComponent<freefall>();
            if (anime != null) anime.SetTrigger("Jump");

            StartCoroutine(PlayerJumpMomentum(cc, jumpVelocity, ff));
        }
    }

    IEnumerator DelayedFreefall(freefall ff, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ff != null) ff.ItsFalling = true;
    }

    IEnumerator PlayerJumpMomentum(CharacterController cc, Vector3 velocity, freefall ff)
    {
        float jumpTimer = 0f;
        float jumpDuration = 0.6f;

        while (jumpTimer < jumpDuration)
        {
            jumpTimer += Time.deltaTime;
            velocity.y += -9.81f * Time.deltaTime;

            if (cc != null && cc.enabled)
            {
                cc.Move(velocity * Time.deltaTime);
            }

            yield return null;
        }

        if (ff != null)
        {
            ff.RunForward();
            StartCoroutine(DelayedFreefall(ff, 0.1f));
        }
    }

    IEnumerator IndividualRobotFall(GameObject robot, Vector3 jumpVelocity, int robotIndex)
    {
        var ranger = robot.GetComponent<GalacticRangers>();

        Vector3 forwardDir = robot.transform.forward;
        forwardDir.y = 0;

        if (forwardDir != Vector3.zero)
        {
            robot.transform.rotation = Quaternion.LookRotation(forwardDir, Vector3.up);
        }

        bool falling = true;
        if (ranger != null)
        {
            ranger.IsFreefall = true;
            if (ranger.HeadAnime != null)
            {
                ranger.HeadAnime.SetBool("Run", false);
                ranger.HeadAnime.SetBool("FreeFall", true);
                ranger.HeadAnime.SetBool("Ground", false);
            }
            if (ranger.FootAnime != null)
            {
                ranger.FootAnime.SetBool("Run", false);
                ranger.FootAnime.SetBool("FreeFall", true);
                ranger.FootAnime.SetBool("Ground", false);
            }
        }

        CharacterController cc = robot.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = true;

        float jumpTimer = 0f;
        float jumpDuration = 0.6f;

        while (jumpTimer < jumpDuration)
        {
            jumpTimer += Time.deltaTime;
            jumpVelocity.y += -9.81f * Time.deltaTime;

            robot.transform.rotation = Quaternion.LookRotation(robot.transform.forward, Vector3.up);

            if (cc != null && cc.enabled)
            {
                cc.Move(jumpVelocity * Time.deltaTime);
            }
            else
            {
                robot.transform.position += jumpVelocity * Time.deltaTime;
            }

            yield return null;
        }

        Vector3 initialWorldPos = robot.transform.position;
        float localFallTimer = 0f;

        Transform targetLandingPoint = null;
        if (landingPoints != null && landingPoints.Length > 0)
        {
            targetLandingPoint = landingPoints[robotIndex % landingPoints.Length];
        }

        bool aligningWithTarget = false;

        while (falling)
        {
            localFallTimer += Time.deltaTime;

            float targetX = robot.transform.position.x;
            float targetZ = robot.transform.position.z;

            float distToGround = 50f;
            RaycastHit airHit;
            if (Physics.Raycast(robot.transform.position, Vector3.down, out airHit, 50f, groundLayer))
            {
                distToGround = airHit.distance;
                aligningWithTarget = true;
            }

            float currentFallSpeed = fallSpeed;
            if (distToGround < 10f)
            {
                currentFallSpeed = Mathf.Lerp(slowSpeed, fallSpeed, distToGround / 10f);
                if (ranger != null)
                {
                    if (ranger.HeadAnime != null) ranger.HeadAnime.SetBool("FreeFall", false);
                    if (ranger.FootAnime != null) ranger.FootAnime.SetBool("FreeFall", false);
                }
            }

            if (aligningWithTarget && targetLandingPoint != null)
            {
                float flySmoothSpeed = 1.2f;
                targetX = Mathf.Lerp(robot.transform.position.x, targetLandingPoint.position.x, Time.deltaTime * flySmoothSpeed);
                targetZ = Mathf.Lerp(robot.transform.position.z, targetLandingPoint.position.z, Time.deltaTime * flySmoothSpeed);

                currentFallSpeed = Mathf.Lerp(slowSpeed, fallSpeed, distToGround / 10f);
                if (ranger != null)
                {
                    if (ranger.HeadAnime != null) ranger.HeadAnime.SetBool("FreeFall", false);
                    if (ranger.FootAnime != null) ranger.FootAnime.SetBool("FreeFall", false);
                }
            }
            else
            {
                float waveSpeed = 2.0f;
                float waveAmountX = 1.5f;
                float waveAmountZ = 1.0f;

                float timeOffset = localFallTimer * waveSpeed + (robotIndex * 1.5f);
                float hoverX = Mathf.Sin(timeOffset) * waveAmountX;
                float hoverZ = Mathf.Cos(timeOffset * 0.7f) * waveAmountZ;

                targetX = initialWorldPos.x + hoverX;
                targetZ = initialWorldPos.z + hoverZ;
            }

            float deltaX = targetX - robot.transform.position.x;
            float deltaZ = targetZ - robot.transform.position.z;
            float deltaY = -currentFallSpeed * Time.deltaTime;

            Vector3 moveDirection = new Vector3(deltaX, deltaY, deltaZ);

            if (cc != null && cc.enabled)
            {
                cc.Move(moveDirection);

                if (aligningWithTarget && cc.isGrounded && localFallTimer > 0.5f)
                {
                    falling = false;
                    if (ranger != null)
                    {
                        ranger.IsFreefall = false;
                        if (ranger.HeadAnime != null) ranger.HeadAnime.SetBool("Ground", true);
                        if (ranger.FootAnime != null) ranger.FootAnime.SetBool("Ground", true);
                    }

                    if (targetLandingPoint != null)
                    {
                        robot.transform.position = targetLandingPoint.position;
                        robot.transform.rotation = targetLandingPoint.rotation;

                        SplineContainer spline = targetLandingPoint.GetComponent<SplineContainer>();
                        if (spline != null && ranger != null)
                        {
                            StartCoroutine(FollowSplineAfterLanding(robot, spline, ranger));
                            yield break;
                        }
                    }

                    yield return new WaitForSeconds(0.05f);
                    if (ranger != null)
                    {
                        ranger.RangersModeActive = true;
                        ranger.enabled = true;
                    }
                    yield break;
                }
            }
            else
            {
                robot.transform.position += moveDirection;
            }

            yield return null;
        }
    }

    IEnumerator FollowSplineAfterLanding(GameObject robot, SplineContainer spline, GalacticRangers ranger)
    {
        if (ranger != null)
        {
            if (ranger.HeadAnime != null) ranger.HeadAnime.SetBool("Run", true);
            if (ranger.FootAnime != null) ranger.FootAnime.SetBool("Run", true);
        }

        CharacterController cc = robot.GetComponentInChildren<CharacterController>();

        float splineProgress = 0f;
        float runSpeedOnSpline = 0.1f;

        while (splineProgress < 1f)
        {
            splineProgress += Time.deltaTime * runSpeedOnSpline;

            Vector3 targetPosition = spline.EvaluatePosition(splineProgress);
            Vector3 targetTangent = spline.EvaluateTangent(splineProgress);

            if (targetTangent != Vector3.zero)
            {
                robot.transform.rotation = Quaternion.LookRotation(targetTangent);
            }

            if (cc != null && cc.enabled)
            {
                Vector3 moveDelta = targetPosition - robot.transform.position;
                moveDelta.y -= 9.81f * Time.deltaTime;
                cc.Move(moveDelta);
            }
            else
            {
                robot.transform.position = targetPosition;
            }

            yield return null;
        }

        if (ranger != null)
        {
            if (ranger.HeadAnime != null) ranger.HeadAnime.SetBool("Run", false);
            if (ranger.FootAnime != null) ranger.FootAnime.SetBool("Run", false);
            ranger.RangersModeActive = true;
            ranger.enabled = true;
        }
    }
}