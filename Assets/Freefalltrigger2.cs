using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freefalltrigger2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public LayerMask layer;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Starta fallet!
            freefall fallScript = other.GetComponent<freefall>();
            //fallScript.ItsFalling = false;
           // fallScript.Glide();
            //fallScript.RunFalse(); // Startar ljudet också
        }
    }

    // Lägg detta i ett litet skript på din Trigger-zon eller i freefall-skriptet
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Starta fallet!
            freefall fallScript = other.GetComponent<freefall>();
            fallScript.ItsFalling = true;
            fallScript.RunForward(); // Startar ljudet också
        }

        if(other.CompareTag("Ground"))
        {
            freefall fallScript = other.GetComponent<freefall>();
            HelikopterController.Instance.CancelHelikopter();
        }
    }
}
