using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class InfoText : MonoBehaviour
{

    public GameObject InfoText_;

    public static InfoText instance;
    public string Text_;
    public GameObject Info2_;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

    }

    // Update is called once per frame
    void Update()
    {
        Text_ = InfoText_.GetComponentInChildren<TMP_Text>().text;

        if (InfoText_.activeSelf)
        {
            StartCoroutine(WaitText());
        }
        else if (Info2_.activeSelf)
        {
            StartCoroutine(WaitText());
        }
    }

    public void SetText(string text)
    {
        if (InfoText_.activeSelf)
        {
            InfoText_.GetComponentInChildren<TMP_Text>().text = text;

        }
        else if (Info2_.activeSelf)
        {
            Info2_.GetComponentInChildren<TMP_Text>().text = text;
        }

    }

    IEnumerator WaitText()
    {

        yield return new WaitForSeconds(5);

        if(InfoText_.activeSelf)
        {
            InfoText_.SetActive(false);
        }
        else if(Info2_.activeSelf)
        {
            Info2_.SetActive(false);
        }
    }

   
}
