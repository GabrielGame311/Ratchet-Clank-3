using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoInstructionsUI : MonoBehaviour
{

    public TMP_Text text_;
    public static InfoInstructionsUI instance;
    public GameObject Instruction;
    public int ItsGame;
    public KeyCode Key_;
    public float TimeCount;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(Instruction.activeSelf)
        {
            if (Input.GetKeyDown(Key_))
            {
                HideInstruction();


            }
        }

        
    }

    public void SetInstruction(string text)
    {
        text_.text = text;
        Instruction.SetActive(true);

        StartCoroutine(wait());
        if(ItsGame == 1)
        {
            GameThyrra_UI.Instance.GamePanel.SetActive(false);
        }

    }

    IEnumerator wait()
    {


        yield return new WaitForSeconds(TimeCount);


        HideInstruction();

    }




    public void HideInstruction()
    {


        Instruction.SetActive(false);

        if (ItsGame == 1)
        {
            GameThyrra_UI.Instance.GamePanel.SetActive(true);
        }
            

    }
}
