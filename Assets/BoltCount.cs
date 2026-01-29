using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoltCount : MonoBehaviour
{

    public int countBolt;
    public TMP_Text BoltText;

    public Bolts bolts;
    
    // Start is called before the first frame update
    void Start()
    {
        BoltText.text = "";

        bolts.bolt = countBolt;

        countBolt.ToString("+" + BoltText.text);

        StartCoroutine(wait());
    }

    // Update is called once per frame
    void Update()
    {
       

        
    }




    IEnumerator wait()
    {
        yield return new WaitForSeconds(5);

        countBolt = bolts.bolt;

        yield return new WaitForSeconds(001);

        countBolt -= countBolt;

    }
}
