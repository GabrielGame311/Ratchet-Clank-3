using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Wrench : MonoBehaviour
{

    private GameObject WrenchObj;

    
    
    public AudioSource wrnechsound;
    Animator anime;
    public float moveSpeed = 5f;
    public float attackMoveDistance = 1f;
    private Vector3 targetPosition;
    private Vector3 moveDirection;
    public Transform player;
    public bool Isplaying = false;
    public int damage;
    public RatchetController controller;
    public ParticleSystem particleSplash;
    
    private bool isMoving = false;
    PlayerControlls controls;
    private bool isReturning = false;
   
    
    float wrenchtime = 2;
    bool isPlaying = false;
    public Quaternion rot;
    Vector3 poss;
    public Transform Object;
    Transform ps;
    public Transform handPosition;
    public float throwSpeed;
    public float returnSpeed;
    public float returnDelay;
    public float throwDuration;
    public float returnDuration;
    Vector3 pos;
    private Vector3 initialPosition;
    float returnTime = 0f;
    bool moveback = false;



    public float autoAimRadius = 2f;
    public float autoAimAngle = 60f;
    public LayerMask enemyLayer;
    public float lungeDistance = 1f;
    public Animator animeWrench;

    private void Start()
    {
        ps = GetComponentInParent<WeaponSwitcher>().transform;
        pos = transform.position;
        rot = Object.transform.rotation;
        controls = new PlayerControlls();

        WrenchObj = GetComponent<GameObject>();


        anime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();

        controller = GameObject.FindObjectOfType<RatchetController>();
        
        
    }


    private void Update()
    {

        anime.SetBool("Gun", false);
        if (gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.F) && !isMoving)
            {
                StartAttackMove();
                TryAutoTargetAndAttack();
            }

            if (isMoving)
            {
                MoveForward();
            }


            if (Input.GetKeyDown(KeyCode.Mouse1) && !isReturning)
            {
                anime.SetTrigger("ThrowWrench");
                //StartCoroutine(ThrowWrench());
            }
        }

          


       

    }

    void TryAutoTargetAndAttack()
    {
        // 1. Leta efter fiender i närheten (inom sfär)
        Collider[] enemies = Physics.OverlapSphere(player.transform.position, autoAimRadius, enemyLayer);

        Transform closestEnemy = null;
        float closestAngle = autoAimAngle;

        foreach (var enemy in enemies)
        {
            Vector3 dirToEnemy = (enemy.transform.position - player.transform.position).normalized;
            float angle = Vector3.Angle(player.transform.forward, dirToEnemy);

            if (angle < closestAngle)
            {
                closestAngle = angle;
                closestEnemy = enemy.transform;
            }
        }

        // 2. Om vi hittar en fiende i sikte
        if (closestEnemy != null)
        {
            // Rota mot fienden
            Vector3 lookDir = (closestEnemy.position - player.transform.position).normalized;
            lookDir.y = 0;
            player.transform.rotation = Quaternion.LookRotation(lookDir);

            // Flytta spelaren lite framåt (lunge)
            Vector3 lungeTarget = player.transform.position + lookDir * lungeDistance;
            player.GetComponent<CharacterController>().Move(lookDir * lungeDistance);
        }

        // 3. Spela attackanimation
        animeWrench.SetTrigger("Hit");

        // 4. Du kan koppla ett Animation Event som kallar på WrenchHit() vid rätt timing
    }

    public void Throw1()
    {
        StartCoroutine(ThrowWrench());
    }

    private IEnumerator ThrowWrench()
    {
        // Record initial position
        initialPosition = transform.position;

        // Move wrench forward
        float throwTime = 0f;
        while (throwTime < throwDuration)
        {
            transform.localPosition += transform.forward * throwSpeed * Time.deltaTime;
            throwTime += Time.deltaTime;
            Object.transform.Rotate(0f, 10f, 0f);
            transform.parent = null;
            yield return null;
        }

        // Wait before returning wrench
        yield return new WaitForSeconds(returnDelay);
        //Object.transform.Rotate(0f, 7f, 0f);
        // Move wrench back to hand
        moveback = true;
        StartCoroutine(MoveWrenchBackToHandCoroutine());

        // Snap wrench to hand

    }

    private IEnumerator MoveWrenchBackToHandCoroutine()
    {

        while (moveback)
        {
            transform.position = Vector3.MoveTowards(transform.localPosition, handPosition.transform.position, throwSpeed * Time.deltaTime);
            Object.transform.Rotate(0f, 10f, 0f);
            isPlaying = true;
            if (Vector3.Distance(handPosition.transform.position, gameObject.transform.position) < 0.6f)
            {
                transform.SetParent(ps);
                ;
                
                moveback = false;
                isPlaying = false;

                Object.transform.localRotation = Quaternion.identity;

                transform.position = handPosition.position;
                transform.rotation = handPosition.rotation;
                transform.localScale = handPosition.localScale;
                

            }
          
            yield return null;
        }



    }


    void StartAttackMove()
    {
        isMoving = true;
        isPlaying = true;
        animeWrench.SetTrigger("Hit");
        // Räkna ut målpositionen framåt
        targetPosition = player.transform.position + player.transform.forward * attackMoveDistance;

        // Starta attackanimationen
        anime.SetTrigger("Wrench");
    }

    void MoveForward()
    {
        // Räkna ut rörelsesteg denna frame
        Vector3 moveStep = moveSpeed * Time.deltaTime * player.transform.forward;

        // Flytta spelaren
        GameObject.FindObjectOfType<RatchetController>().MyController.Move(moveStep);

        // Kolla om spelaren har nått eller passerat målpositionen
        if (Vector3.Distance(player.transform.position, targetPosition) <= moveStep.magnitude)
        {
            // Sätt exakt position till målet
            player.transform.position = targetPosition;

            isMoving = false;
            isPlaying = false;
        }
    }

    private void OnEnable()
    {
        anime.SetBool("Gun", false);
       

    }


    public void EndAnime()
    {
        isMoving = false;
        isPlaying = false;
        player.transform.position = targetPosition;
    }


    private void OnTriggerEnter(Collider other)
    {
        
        //Box
        



        if(other.CompareTag("Box"))
        {
           

            if(Isplaying == true)
            {
                other.GetComponent<Box>().Break(damage);
                Isplaying = false;
            }
            


        }
        else if(other.CompareTag("Health"))
        {

            if (Isplaying == true)
            {

                other.GetComponent<HealthItem>().Break(damage);
                Isplaying = false;
            }

            
            
        }
        else if(other.CompareTag("AmmoBox"))
        {
            if(Isplaying == true)
            {
                other.GetComponent<AmmoCrate>().Break(damage);
                Isplaying = false;
            }
        }
        else if (other.CompareTag("Enemie"))
        {
            if (Isplaying == true)
            {
                other.GetComponent<EnemiesHealth>().TakeDamage(damage);
                Isplaying = false;
            }
        }




    }



   



}
