using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class TriggerInstructions : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public string[] InstructionText;

    public int currentInstruction;

    public string InstructionType;
    public AudioClip[] soundfx;
    public float TimeTrigger;
    public bool StartInstruction = false;
    public bool isTrigger = false;
    public bool IsDestoryable = true;
    public MonoBehaviour DontDestroyScript;
    int startCurrent;
    void Start()
    {
       
        if (InstructionType == "Start")
        {
            StartInstruction = true;
        }

    }

    private void OnDestroy()
    {
        if(InstructionType == "Destroy")
        {
                if (soundfx != null && soundfx.Length > currentInstruction && soundfx[currentInstruction] != null)
                {
                    InfoInstructionsUI.instance.sound.clip = soundfx[currentInstruction];
                    InfoInstructionsUI.instance.SetInstruction(InstructionText[currentInstruction]);
                    InfoInstructionsUI.instance.sound.Play();
                    InfoInstructionsUI.instance.Istrigger = true;
                }
                else
                {
                    InfoInstructionsUI.instance.sound.clip = null;
                    InfoInstructionsUI.instance.SetInstruction(InstructionText[currentInstruction]);
                }
        }
        if (InstructionType == "Trigger")
        {
            if (isTrigger)
            {
               // InfoInstructionsUI.instance.HideInstruction();
                
                //InfoInstructionsUI.instance.anime.SetTrigger("Open");
            }
        }

    }

    private void OnDisable()
    {
        if (InstructionType == "Destroy")
        {
            InfoInstructionsUI.instance.sound.clip = soundfx[currentInstruction];
            InfoInstructionsUI.instance.SetInstruction(InstructionText[currentInstruction]);
            StartInstruction = true;
        }
    }

    // Update is called once per frame
    void Update()
    {


        if (InstructionType == "Space")
        {
            if(isTrigger)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                   StartInstruction = true;
                   InfoInstructionsUI.instance.Istrigger = true;
                    if (IsDestoryable)
                    {
                        Destroy(this);
                    }
                      
                }
            }
        }
        
        if (InstructionType == "Trigger")
        {
            if (isTrigger)
            {
                if (soundfx != null && soundfx.Length > currentInstruction && soundfx[currentInstruction] != null)
                {
                    InfoInstructionsUI.instance.sound.clip = soundfx[currentInstruction];
                    InfoInstructionsUI.instance.SetInstruction(InstructionText[currentInstruction]);
                    InfoInstructionsUI.instance.sound.Play();
                    InfoInstructionsUI.instance.Istrigger = true;
                }
                else
                {
                    InfoInstructionsUI.instance.Istrigger = false;
                    InfoInstructionsUI.instance.sound.clip = null;
                    InfoInstructionsUI.instance.SetInstruction(InstructionText[currentInstruction]);
                }

                //StartInstruction = true;

                if(IsDestoryable)
                {
                    Destroy(this);
                    
                }
                Destroy(GetComponent<TriggerInstructions>(), TimeTrigger);
            }
        }

        if(StartInstruction)
        {


           InfoInstructionsUI.instance.SetInstruction(InstructionText[currentInstruction]);

            StartInstruction = false;
        }

        if (!InfoInstructionsUI.instance.sound.isPlaying && isTrigger && soundfx != null && soundfx.Length > currentInstruction && soundfx[currentInstruction] != null)
        {
            // Finns det fler instruktioner kvar i listan?
            if (InstructionText.Length > currentInstruction)
            {
                // 1. S�tt clip och text f�r nuvarande index (b�rjar p� 0)
                InfoInstructionsUI.instance.sound.clip = soundfx[currentInstruction];
                InfoInstructionsUI.instance.SetInstruction(InstructionText[currentInstruction]);

                // 2. Starta ljudet

                if(InfoInstructionsUI.instance.sound != null)
                {
                    
                  InfoInstructionsUI.instance.sound.Play();
                }

                // 3. �ka indexet direkt s� att vi �r redo f�r n�sta rad n�r detta ljud �r klart
                currentInstruction++;
            }
            else
            {
                // Om listan �r slut (currentInstruction har n�tt InstructionText.Length)
                // St�ng av skriptet helt.
                enabled = false;
            }
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
           
            if(!isTrigger) 
            {
                
              
                if (InstructionType == "Destroy") 
                {

                }
                else 
                {
                    isTrigger = true;
                }
                
            }
            //GetComponent<Collider>().enabled = false;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
           

            if (InstructionType == "Trigger")
            {
                
            }
            else
            {
                //isTrigger = false;
            }

        }
    }
}
