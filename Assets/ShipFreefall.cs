using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Importera Unitys Spline-system
using UnityEngine.Splines;

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

    [Header("Landing Targets (Dra in dina LandingPoints här)")]
    public Transform[] landingPoints;

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
        // FIX: Varje karaktär mäter sitt EGET avstånd till dörren (character istället för player)
        while (Vector3.Distance(new Vector3(0, 0, character.transform.position.z), new Vector3(0, 0, doorPosition.position.z)) > 0.3f)
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
        if (cc != null) cc.enabled = true;

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

            float lookAheadDistance = 50f;
            RaycastHit airHit;

            if (Physics.Raycast(robot.transform.position, Vector3.down, out airHit, lookAheadDistance, groundLayer))
            {
                aligningWithTarget = true;
            }

            if (aligningWithTarget && targetLandingPoint != null)
            {
                float flySmoothSpeed = 1f;
                targetX = Mathf.Lerp(robot.transform.position.x, targetLandingPoint.position.x, Time.deltaTime * flySmoothSpeed);
                targetZ = Mathf.Lerp(robot.transform.position.z, targetLandingPoint.position.z, Time.deltaTime * flySmoothSpeed);
                robot.transform.rotation = Quaternion.Slerp(robot.transform.rotation, targetLandingPoint.rotation, Time.deltaTime * flySmoothSpeed);
            }
            else
            {
                float waveSpeed = 1.5f;
                float waveAmountX = 1.5f;
                float waveAmountZ = 1.0f;

                float timeOffset = localFallTimer * waveSpeed + (robotIndex * 1.5f);
                float hoverX = Mathf.Sin(timeOffset) * waveAmountX;
                float hoverZ = Mathf.Cos(timeOffset * 0.7f) * waveAmountZ;

                targetX = initialWorldPos.x + hoverX;
                targetZ = initialWorldPos.z + hoverZ;

                robot.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            }

            float deltaX = targetX - robot.transform.position.x;
            float deltaZ = targetZ - robot.transform.position.z;
            float deltaY = -fallSpeed * Time.deltaTime;

            Vector3 moveDirection = new Vector3(deltaX, deltaY, deltaZ);

            if (cc != null && cc.enabled)
            {
                cc.Move(moveDirection);

                // FIX: Kräver att roboten har fallit i minst 0.2 sekunder innan isGrounded får aktiveras.
                // Detta förhindrar att den triggas av skeppets golv precis när den hoppar ut!
                if (cc.isGrounded && localFallTimer > 0.2f)
                {
                    falling = false;
                    StartCoroutine(SlowDownAndStop(robot, targetLandingPoint));
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

    IEnumerator SlowDownAndStop(GameObject robot, Transform landingTarget)
    {
        var ranger = robot.GetComponent<GalacticRangers>();

        ranger.HeadAnime.SetBool("FreeFall", false);
        ranger.FootAnime.SetBool("FreeFall", false);

        if (landingTarget != null)
        {
            robot.transform.rotation = landingTarget.rotation;

            // =========================================================================
            // SPLINE-INTEGRERING (Kolla om landningspunkten har en SplineContainer)
            // =========================================================================
            SplineContainer spline = landingTarget.GetComponent<SplineContainer>();

            if (spline != null)
            {
                StartCoroutine(FollowSplineAfterLanding(robot, spline, ranger));
                yield break;
            }
        }

        yield return new WaitForSeconds(0.05f);
        ranger.RangersModeActive = true;
        ranger.enabled = true;
    }

    IEnumerator FollowSplineAfterLanding(GameObject robot, SplineContainer spline, GalacticRangers ranger)
    {
        ranger.HeadAnime.SetBool("Run", true);
        ranger.FootAnime.SetBool("Run", true);

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

        ranger.HeadAnime.SetBool("Run", false);
        ranger.FootAnime.SetBool("Run", false);

        ranger.RangersModeActive = true;
        ranger.enabled = true;
    }
}