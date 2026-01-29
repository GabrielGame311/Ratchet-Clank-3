using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePlay : MonoBehaviour
{

    public GameObject[] Fires;

    public float FireTime;
    float starttime;

    public int FiresInt;
    // Start is called before the first frame update
    void Start()
    {
        starttime = FireTime;
    }

    // Update is called once per frame
    void Update()
    {
        FireTime -= Time.deltaTime; // Decrease timer by time passed

        if (FireTime < 0)
        {
            StartFire();
            FireTime = starttime; // Reset the timer
        }
    }

    void StartFire()
    {
        // Stop the previous fire
        if (FiresInt > 0)
        {
            Fires[FiresInt - 1].GetComponent<FirePlane>().FireStop();
        }
        else if (FiresInt == 0 && Fires.Length > 0) // Handle the first fire
        {
            Fires[Fires.Length - 1].GetComponent<FirePlane>().FireStop();
        }

        // Start the current fire
        Fires[FiresInt].GetComponent<FirePlane>().FirePlay();

        // Increment the index
        FiresInt++;

        // Reset FiresInt if it exceeds array bounds
        if (FiresInt >= Fires.Length)
        {
            FiresInt = 0;
        }
    }

}
