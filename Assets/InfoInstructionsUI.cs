using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;

public class InfoInstructionsUI : MonoBehaviour
{

    public TMP_Text text_;
    public static InfoInstructionsUI instance;
    public GameObject Instruction;
    public int ItsGame;
    public KeyCode Key_;
    public float TimeCount;
    public bool Istrigger = false;
    public AudioSource sound;

    public Animator anime;
    bool isplaying = false;
    // Start is called before the first frame update
    void Start()
    {
        //anime = GetComponentInChildren<Animator>();
        instance = this;
        sound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Instruction.activeSelf)
        {
            if (Input.GetKeyDown(Key_))
            {
                //HideInstruction();


            }
        }
        if (sound.isPlaying)
        {
            Instruction.SetActive(true);
            if (ItsGame == 1)
            {
                GameThyrra_UI.Instance.GamePanel.SetActive(false);
            }
            
        }
        else 
        {
            sound.clip = null;
            if(Istrigger) 
            {
                  
                        anime.SetTrigger("Open");
                        StartCoroutine(wait2());  
                           
                    
                   Istrigger = false;
            }
            
          
            if (ItsGame == 1)
            {
                //GameThyrra_UI.Instance.GamePanel.SetActive(true);
            }
        }

       
    }



        IEnumerator wait2()
        {
            yield return new WaitForSeconds(0.5f);
            Instruction.SetActive(false);
            isplaying = false;
        }

    public void SetInstruction(string text)
    {



        text_.text = text;

        if(sound.clip != null)
        {
            sound.Play();

        }
        else
        {
            StartCoroutine(wait());
        }

      

       





           
       

    }

    IEnumerator wait()
    {

        Instruction.SetActive(true);
        yield return new WaitForSeconds(8);
        anime.SetTrigger("Open");
        yield return new WaitForSeconds(0.5f);
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
