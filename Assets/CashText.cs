using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class CashText : MonoBehaviour
{
    public int Cash_;
    public TMP_Text Cash_Text;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Cash_Text.text = Cash_.ToString();
    }
}
