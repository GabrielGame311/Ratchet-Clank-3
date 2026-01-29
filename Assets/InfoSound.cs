using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class InfoSound : MonoBehaviour
{

    AudioSource sound;
    public AudioClip[] SoundFX;
    public string[] TextInfo;
    public int SoundCount;

    public float TimeRead;
    public KeyCode[] KeyBindings;


    // Start is called before the first frame update
    void Start()
    {

        sound = GetComponent<AudioSource>();




        SoundPlay();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void SoundPlay()
    {
        InfoInstructionsUI.instance.TimeCount = TimeRead;

        string GetNiceKeyName(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.Mouse0: return "Mouse Left";
                case KeyCode.Mouse1: return "Mouse Right";
                case KeyCode.Mouse2: return "Mouse Middle";
                case KeyCode.Keypad0: return "Mouse Left";
                case KeyCode.Keypad1: return "Mouse Right";
                case KeyCode.Space: return "Spacebar";
                case KeyCode.Return: return "Enter";
                // Lägg till fler om du vill
                default: return key.ToString();
            }
        }


        sound.clip = SoundFX[SoundCount];
        sound.Play();

        string instruction = TextInfo[SoundCount];

        int keyIndex = 0;
        while (instruction.Contains("**") && keyIndex < KeyBindings.Length)
        {
            instruction = ReplaceFirst(instruction, "**", GetNiceKeyName(KeyBindings[keyIndex]));
            keyIndex++;
        }

        InfoInstructionsUI.instance.SetInstruction(instruction);
        

        SoundCount++;







    }

    // Hjälpmetod: Ersätter första förekomsten av en textsträng
    string ReplaceFirst(string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0) return text;
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

}
