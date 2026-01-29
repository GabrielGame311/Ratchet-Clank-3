using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadDamage : MonoBehaviour
{

    public float speed = 2f;             // Hastighet för upp- och nedåtgående rörelse
    public float moveDistance = 3f;      // Hur högt objektet ska röra sig uppåt innan det vänder
    public float waitTime = 3f;          // Väntetid mellan rörelser
    public float WaitTimeObject;
    public int Damage = 20;
    private Vector3 startPos;            // Startpositionen för att återgå vid nedåtgående rörelse

    void Start()
    {
        // Spara startpositionen för att kunna återgå dit
        startPos = transform.position;

        // Starta rörelse-loopen
        StartCoroutine(MovePlatform());
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            other.GetComponent<QuarkController>().TakeDamage(Damage);
        }
    }

    IEnumerator MovePlatform()
    {

        yield return new WaitForSeconds(WaitTimeObject);

        while (true)
        {
            // Beräkna målet för att röra sig uppåt
            Vector3 upPosition = startPos + Vector3.up * moveDistance;

            // Flytta uppåt
            while (Vector3.Distance(transform.position, upPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, upPosition, speed * Time.deltaTime);
                yield return null;
            }

            // Säkerställ att den är exakt vid målet
            transform.position = upPosition;

            // Flytta neråt tillbaka till startpositionen
            while (Vector3.Distance(transform.position, startPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, startPos, speed * Time.deltaTime);
                yield return null;
            }

            // Säkerställ att den är exakt vid startpositionen
            transform.position = startPos;

            // Vänta i 3 sekunder innan nästa rörelse
            yield return new WaitForSeconds(waitTime);
        }
    }
}
