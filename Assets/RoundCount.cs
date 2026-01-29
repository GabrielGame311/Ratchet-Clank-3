using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;




public class RoundCount : MonoBehaviour
{

    public int RoundCount_;
    public TMP_Text CountText;
    public int MaxCount;
    public GameObject roundcount;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

       

        CountText.text = RoundCount_.ToString() + "/ " + MaxCount;
    }
}
