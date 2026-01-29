using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuarkController : MonoBehaviour
{

    //Movment
    public float moveSpeed = 5f;      // Rörelsehastighet
    public float gravity = -9.81f;    // Gravitationseffekt
    public float rotationSpeed = 10f; // Hastighet för att rotera spelaren mot höger eller vänster
    public Animator anime;
    private Vector3 moveDirection;
    Vector3 mov;
    private CharacterController controller;
    private bool jumpRequest = false;
    //Health
    public bool IsGravity = true;
    public int Health;
    public int MaxHealth;
    public int damageAmount = 10;
    public float attackRange = 1.5f;
    public LayerMask opponentLayer;

    //Jump
    public float JumpSpeed;


    private void Start()
    {
        MaxHealth = Health;
        // Hämtar referensen till CharacterController-komponenten
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {


       

        // Fångar upp den horisontella inputen (höger/vänster)
        float h = Input.GetAxis("Horizontal");

        // Ställer in rörelseriktningen endast på x-axeln
        moveDirection = new Vector3(-h * moveSpeed, 0, 0);

        // Tillämpa gravitationen på y-axeln
        if(IsGravity)
        {
            mov.y -= gravity * Time.deltaTime;
        }

        // Flytta spelaren med CharacterController
        controller.Move(moveDirection * Time.deltaTime);
        controller.Move(mov * Time.deltaTime);



        // Kontrollera om spelaren är på marken
        // Kontrollera om spelaren är på marken
       
        if(controller.isGrounded)
        {

            // Om spelaren trycker på mellanslag, sätt hoppbegäran till true
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jumps();

            }
        }
      
       
        // Utför hoppet om det finns en hoppbegäran och spelaren är på marken
        
          
        



        // Korrigerad rotation beroende på input
        if (h < 0) // Om spelaren går åt höger
        {
            Quaternion targetRotation = Quaternion.Euler(0, 90, 0); // Rotera mot höger (90 grader)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else if (h > 0) // Om spelaren går åt vänster
        {
            Quaternion targetRotation = Quaternion.Euler(0, -90, 0); // Rotera mot vänster (-90 grader)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Aktivera eller inaktivera springanimationen
        if (Mathf.Abs(h) > 0) // Om det finns horisontell rörelse
        {
            anime.SetBool("Run", true); // Aktivera springanimation
        }
        else
        {
            anime.SetBool("Run", false); // Inaktivera springanimation
        }

        if (transform.position.y < 0)
        {
            TakeDamage(200);
        }

       

        GameObject.FindObjectOfType<HealthUI>().UpdateHealthUI();

        //Hit

        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            anime.SetTrigger("Hit");
            ApplyDamage();
        }

    }

    public void ApplyDamage()
    {
        // Check if any opponents are within the attack range
        Collider[] hitOpponents = Physics.OverlapSphere(transform.position, attackRange, opponentLayer);

        foreach (Collider opponent in hitOpponents)
        {
            // Attempt to get the opponent's health component
            EnemiesHealth health = opponent.GetComponent<EnemiesHealth>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);
            }
        }
    }

    // Optional: Show attack range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    public void Jumps()
    {
        mov.y = JumpSpeed;
        anime.SetTrigger("Jump");
    }
    public void Jumps2()
    {
        mov.y = JumpSpeed;
       
    }


    public void TakeDamage(int damage)
    {
       
        Health -= damage;

       

        // Uppdatera UI för hälsan
        

        if (Health < 0)
        {
            Die();
        }

    }

    void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
