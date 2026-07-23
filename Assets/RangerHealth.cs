using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangerHealth : MonoBehaviour
{

    public float Health;
    public float MaxHealth;
    public GameObject CrackedRanger;
    public float verticalAngle = 0f;
    public bool Explode = false;
    public float RangerDistance;
    public float ExplodeTime;
    GameObject Player_;
    public Animator animeHead;
    public Animator animeFoot;
    public MonoBehaviour[] ScriptsDisabled;
    bool YesSir = false;

    public bool IsDamageable = false;

    // Start is called before the first frame update
    void Start()
    {

        Player_ = GameObject.FindGameObjectWithTag("Player");
        MaxHealth = Health;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        Vector3 rayDirection = Quaternion.AngleAxis(verticalAngle, transform.right) * transform.forward;

        // Vi kastar str�len med den nya riktningen (rayDirection)
        if (Physics.Raycast(transform.position, rayDirection, out hit, RangerDistance))
        {
            // Flyttade Debug.DrawRay hit s� den anv�nder den faktiska riktningen
            Debug.DrawRay(transform.position, rayDirection * RangerDistance, Color.red);


            if(!YesSir)
            {

                if (hit.collider.gameObject == Player_)
                {
                    foreach (MonoBehaviour pl in ScriptsDisabled)
                    {
                        pl.enabled = false;
                    }
                    animeHead.SetTrigger("Sir");
                    animeFoot.SetTrigger("Sir");
                    animeHead.SetBool("Walk", false);
                    animeFoot.SetBool("Walk", false);
                    StartCoroutine(wait());
                    YesSir = true;
                }
                
                
            }

        }
        else
        {
            // Tips: Rita str�len som gr�n n�r den INTE tr�ffar n�got, s� ser du i Scene-vyn var den pekar!
            Debug.DrawRay(transform.position, rayDirection * RangerDistance, Color.green);
        }




        if (Explode)
        {


           


                Destroy(gameObject, ExplodeTime);
            

        }
    }

    private void OnDrawGizmos()
    {
        
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(4);

        foreach (MonoBehaviour pl in ScriptsDisabled)
        {
            pl.enabled = true;
        }

        yield return new WaitForSeconds(2);

        YesSir = false;
    }

    private void OnDestroy()
    {
        GameObject cracked = Instantiate(CrackedRanger, transform.position, transform.rotation);
        
        Destroy(cracked, 4);
    }

    public void TakeDamage(float damage)
    {
        if(IsDamageable)
        {
                Health -= damage;

            if(Health < 0)
            {
                Health = 0;
                Die();

            }
        }
        
            
        

       



    }


    void Die()
    {


        Destroy(gameObject, 2);
    }
}
