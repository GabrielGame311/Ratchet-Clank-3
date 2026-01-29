using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Bolts : MonoBehaviour
{

    public int bolt;
    public int BoltCount;
    public TMP_Text Bolts_Text;
    public TMP_Text BoltsCounting_Text;
    public float CountTime = 2;
    float starttime;
    public static Bolts Bolt;

    void Start()
    {
        //bolt = PlayerPrefs.GetInt("Bolt", 0);

        Bolt = GetComponent<Bolts>();
        //bolt = 0;

        BoltsCounting_Text.text = "";

        starttime = CountTime;
    }

    // Update is called once per frame
    void Update()
    {
        Bolts_Text.text = bolt.ToString();
        //PlayerPrefs.SetInt("Bolt", bolt);
       // PlayerPrefs.Save();
        if (BoltCount > 0)
        {
            CountTime -= Time.deltaTime;
            BoltsCounting_Text.text = "+" + BoltCount.ToString();
        }

        if (CountTime <= 0)
        {
            StartCoroutine(AddBoltsOverTime(BoltCount, 1));
            CountTime = starttime;
            bolt += BoltCount;
            BoltCount = 0;
            BoltsCounting_Text.text = "";
        }


    }

    IEnumerator AddBoltsOverTime(int boltCount, float duration)
    {
        float elapsed = 0.0f;
        float increment = (float)boltCount / duration;
        int startBolt = bolt;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bolt = (int)(startBolt + increment * elapsed);
            Bolts_Text.text = bolt.ToString();

            yield return null;
        }

        int endBolt = startBolt + boltCount;
        bolt = endBolt;
        Bolts_Text.text = bolt.ToString();
        BoltsCounting_Text.text = "";

        BoltCount = 0;
       

    }







}
