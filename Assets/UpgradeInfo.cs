using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeInfo : MonoBehaviour
{
    public GameObject UpgradeObj;
    public TMP_Text UpgradeText;
    public static UpgradeInfo UpgradeInfo_;

    // Start is called before the first frame update
    void Start()
    {
        UpgradeInfo_ = GetComponent<UpgradeInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        if(UpgradeObj.activeSelf)
        {
            StartCoroutine(Wait());
        }


    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(5);

        UpgradeObj.SetActive(false);
    }
}
