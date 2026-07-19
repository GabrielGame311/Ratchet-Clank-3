using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Importera Unitys Spline-system
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

    [Header("Landing Targets (Dra in dina LandingPoints h�r)")]
    public Transform[] landingPoints;
    public bool WithoutPlayer = false;
    Animator anime;
    CharacterController controller;
    RatchetController playercontroller;
    public AudioSource Sound;

    void Start()
    {
        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        
        player.GetComponent<RatchetController>().cine.m_XAxis.Value = 95;

        // Ignorera kollisioner mellan robotar direkt
        for (int i = 0; i < robots.Length; i++)
        {
            for (int j = i + 1; j < robots.Length; j++)
            {
                if (robots[i] != null && robots[j] != null)
                {
                    Physics.IgnoreCollision(robots[i].GetComponent<Collider>(), robots[j].GetComponent<Collider>());
                    robots[i].GetComponent<GalacticRangers>().IsFreefall = true;
                }
            }
        }

        // St�ll in startposition
        playercontroller = player.GetComponent<RatchetController>();
        playercontroller.CanMove = false;

        controller = player.GetComponentInChildren<CharacterController>();
        controller.enabled = false;
        player.transform.position = SpawnPoint.transform.position;
        player.transform.rotation = SpawnPoint.transform.rotation;
       
        controller.enabled = true;

        // Starta sekvensen
        if(WithoutPlayer)
        {
            //player = null;
            player.GetComponent<freefall>().ItsFalling = true;
            StartCoroutine(MoveRobots());
        }
        else
        {
            StartCoroutine(MoveRobotsThenPlayer());
        }
    }


    IEnumerator MoveRobots()
    {
       

        yield return new WaitForSeconds(2);

        

        // 1. Starta ALLA robotars r�relse f�rst s� att de hamnar l�ngst fram
        for (int i = 0; i < robots.Length; i++)
        {
            if (robots[i] != null)
            {
                StartCoroutine(MoveToDoorAndJump(robots[i], true, i));
            }
        }

       

        yield return null;
    }
    IEnumerator MoveRobotsThenPlayer()
    {
       

        yield return new WaitForSeconds(2);

        Sound.Play();

        // 1. Starta ALLA robotars r�relse f�rst s� att de hamnar l�ngst fram
        for (int i = 0; i < robots.Length; i++)
        {
            if (robots[i] != null)
            {
                StartCoroutine(MoveToDoorAndJump(robots[i], true, i));
            }
        }

        // 2. En kort f�rdr�jning s� robotarna hinner springa f�re spelaren ut mot d�rren
        yield return new WaitForSeconds(0.4f);

        // 3. Starta spelaren sist s� han hamnar bakom robotarna
        StartCoroutine(MoveToDoorAndJump(player, false, -1));

        yield return null;
    }

    IEnumerator MoveToDoorAndJump(GameObject character, bool isRobot, int robotIndex)
    {
        CharacterController cc = character.GetComponentInChildren<CharacterController>();

        float timer = 0f;
        float maxTime = 2.5f; // S�kerhetsgr�ns

        // Tvinga nollst�llning av r�relse vid start
        if (cc != null && cc.enabled)
        {
            cc.Move(Vector3.zero);
        }

        // K�r loopen tills vi �r framme eller tiden g�r ut
        while (Vector3.Distance(new Vector3(0, 0, character.transform.position.z), new Vector3(0, 0, doorPosition.position.z)) > 0.3f && timer < maxTime)
        {
            timer += Time.deltaTime;

            // R�relsevektor
            Vector3 direction = character.transform.forward * speed;
            direction.y = -9.81f; // Applicera konstant gravitationskraft

            // S�tt l�p-animationer
            if (!isRobot)
            {
                anime.SetBool("Run", true);
            }
            else
            {
                var ranger = character.GetComponent<GalacticRangers>();
                if (ranger != null)
                {
                    ranger.RangersModeActive = false;
                    ranger.HeadAnime.SetBool("Run", true);
                    ranger.FootAnime.SetBool("Run", true);
                }
            }

            // Utf�r r�relse
            if (cc != null && cc.enabled)
            {
                cc.Move(direction * Time.deltaTime);
            }
            else
            {
                character.transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.World);
            }

            yield return null;
        }

        // Timeout-varning
        if (timer >= maxTime)
        {
            Debug.LogWarning(character.name + " fastnade vid d�rren!");
        }

        // ========================================================
        // ST�NG AV L�PNING OCH F�RBERED HOPP
        // ========================================================
        if (!isRobot)
        {
            anime.SetBool("Run", false);
        }
        else
        {
            var ranger = character.GetComponent<GalacticRangers>();
            if (ranger != null)
            {
                ranger.HeadAnime.SetBool("Run", false);
                ranger.FootAnime.SetBool("Run", false); // �ndrade fr�n "FootAnime" till false
            }
        }

        // Ber�kna hopp-vektor
        float jumpForce = 5f;
        float jumpUp = 4f;
        Vector3 jumpVelocity = (character.transform.forward * jumpForce) + (Vector3.up * jumpUp);

        // Starta hopp- eller fall-sekvens
        if (isRobot)
        {
           
            StartCoroutine(IndividualRobotFall(character, jumpVelocity, robotIndex));
        }
        else
        {
            freefall ff = character.GetComponent<freefall>();

            anime.SetTrigger("Jump");

            // Om vi har en CharacterController, skicka med den till momentumet
            StartCoroutine(PlayerJumpMomentum(cc, jumpVelocity, ff));
        }
    }

    IEnumerator DelayedFreefall(freefall ff, float delay)
    {
        yield return new WaitForSeconds(delay);
        ff.ItsFalling = true;
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
        forwardDir.y = 0; // Vi vill inte att de tittar ner i marken

        if (forwardDir != Vector3.zero)
        {
            robot.transform.rotation = Quaternion.LookRotation(forwardDir, Vector3.up);
        }




        bool falling = true;
        ranger.IsFreefall = true;
        ranger.HeadAnime.SetBool("Run", false);
        ranger.FootAnime.SetBool("Run", false);
        ranger.HeadAnime.SetBool("FreeFall", true);
        ranger.FootAnime.SetBool("FreeFall", true);

        // Se till att Ground �r avst�ngd under fallet
        ranger.HeadAnime.SetBool("Ground", false);
        ranger.FootAnime.SetBool("Ground", false);

        CharacterController cc = robot.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = true;

        // HOPP-MOMENTUM UT UR D�RREN (ROBOT)
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

        // STARTA DET FRIA FALLET (ROBOT)
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

            // 1. Ber�kna avst�nd till marken f�r Slow Landing
            float distToGround = 50f;
            RaycastHit airHit;
            if (Physics.Raycast(robot.transform.position, Vector3.down, out airHit, 50f, groundLayer))
            {
                distToGround = airHit.distance;
                aligningWithTarget = true;
            }

            // 2. Slow down logik (under 10m)
            float currentFallSpeed = fallSpeed;
            if (distToGround < 10f)
            {
                currentFallSpeed = Mathf.Lerp(slowSpeed, fallSpeed, distToGround / 10f);
                // St�ng av FreeFall animation n�r vi n�rmar oss marken
                ranger.HeadAnime.SetBool("FreeFall", false);
                ranger.FootAnime.SetBool("FreeFall", false);
            }

           

            if (aligningWithTarget && targetLandingPoint != null)
            {
                float flySmoothSpeed = 1.2f;
                targetX = Mathf.Lerp(robot.transform.position.x, targetLandingPoint.position.x, Time.deltaTime * flySmoothSpeed);
                targetZ = Mathf.Lerp(robot.transform.position.z, targetLandingPoint.position.z, Time.deltaTime * flySmoothSpeed);
                
                currentFallSpeed = Mathf.Lerp(slowSpeed, fallSpeed, distToGround / 10f);
                // St�ng av FreeFall animation n�r vi n�rmar oss marken
                ranger.HeadAnime.SetBool("FreeFall", false);
                ranger.FootAnime.SetBool("FreeFall", false);
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
            float deltaY = -currentFallSpeed * Time.deltaTime; // Anv�nd den justerade hastigheten

            Vector3 moveDirection = new Vector3(deltaX, deltaY, deltaZ);

            if (cc != null && cc.enabled)
            {
                cc.Move(moveDirection);

                if (aligningWithTarget && cc.isGrounded && localFallTimer > 0.5f)
                {
                    falling = false;
                    ranger.IsFreefall = false;

                    // Aktivera Ground animation
                    ranger.HeadAnime.SetBool("Ground", true);
                    ranger.FootAnime.SetBool("Ground", true);

                    if (targetLandingPoint != null)
                    {
                        robot.transform.position = targetLandingPoint.position;
                        robot.transform.rotation = targetLandingPoint.rotation;

                        SplineContainer spline = targetLandingPoint.GetComponent<SplineContainer>();
                        if (spline != null)
                        {
                            StartCoroutine(FollowSplineAfterLanding(robot, spline, ranger));
                            yield break;
                        }
                    }

                    yield return new WaitForSeconds(0.05f);
                    ranger.RangersModeActive = true;
                    ranger.enabled = true;
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