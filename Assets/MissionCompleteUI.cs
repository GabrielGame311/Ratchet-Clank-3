using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;




public class MissionCompleteUI : MonoBehaviour
{

    public TMP_Text text;

    public string MissionComplete_string;
    public static MissionCompleteUI MissionComplete;
    public int Mission = 0;

    public int[] Bolts;

    // Start is called before the first frame update
    void Start()
    {
        MissionComplete = GetComponent<MissionCompleteUI>();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
         text.text = "Mission Accomplished.  Reward: " + Bolts[Mission].ToString() + " bolts";
    }
}
