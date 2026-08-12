using UnityEngine;

public class BouncyStone : MonoBehaviour
{
    [Header("Studs & Fart vid varje landning")]
    [Tooltip("Hur snabbt stenen ska åka framåt varje gång den nuddar marken.")]
    public float forwardSpeed = 20f;

    [Tooltip("Hur högt stenen ska hoppa/studsa upp i luften vid varje landning.")]
    public float bounceForce = 5f;
    public int Damage;
    private Rigidbody rb;
    public LayerMask groundLayer;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Körs varje gång stenen nuddar marken eller ett hinder
        
        if (((1 << collision.gameObject.layer) & groundLayer.value) != 0)
        {
            Vector3 forwardDir = transform.forward;

            // 2. Tvinga stenen att studsa uppåt OCH skjuta framåt igen direkt
            Vector3 newVelocity = (forwardDir * forwardSpeed) + (Vector3.up * bounceForce);
            
            rb.linearVelocity = newVelocity;
        }
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().TakeDamage(Damage);
            collision.gameObject.GetComponent<Player>().anime.SetTrigger("DamageFlatt");
        }
        // 1. Behåll riktningen framåt
        
    }
}