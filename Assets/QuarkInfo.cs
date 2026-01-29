using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarkInfo : MonoBehaviour
{

    public int InfoCount;
    public string[] Info_Text;
    public AudioClip[] InfoSound;
    AudioSource sound;
    bool IsPlaying = false;
    public bool RedToken = false;
   
    // Start is called before the first frame update
    void Start()
    {
        sound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (sound.isPlaying == false)
        {
          
            IsPlaying = false;
            StartCoroutine(wait());
        }
    }



    private void OnTriggerEnter(Collider other)
    {
       
      
          
       
        if(RedToken)
        {
            if (other.tag == "RedToken")
            {

                if (IsPlaying == false)
                {
                    for (int i = 0; i < Info_Text.Length; i++)
                    {
                        InfoCount = i;
                        sound.clip = InfoSound[InfoCount];
                        sound.Play();
                        GameObject.FindObjectOfType<QuarkInfoUI>().SetOnPanel(IsPlaying);
                        GameObject.FindObjectOfType<QuarkInfoUI>().OnText(Info_Text[InfoCount]);
                        IsPlaying = true;
                        
                        RedToken = false;
                    }
                }
               
            }
        }
        else
        {
            if (other.tag == "Cash")
            {

                if (IsPlaying == false)
                {
                    for (int i = 0; i < Info_Text.Length; i++)
                    {
                        // Info_Text[i] = 



                        // InfoCount++;
                     
                        
                        IsPlaying = true;
                        sound.clip = InfoSound[InfoCount];
                        sound.Play();
                        GameObject.FindObjectOfType<QuarkInfoUI>().SetOnPanel(IsPlaying);
                        GameObject.FindObjectOfType<QuarkInfoUI>().OnText(Info_Text[InfoCount]);
                       
                        InfoCount = i;
                       
                        RedToken = true;

                    }
                }
               
            }
        }
        
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(2);

        GameObject.FindObjectOfType<QuarkInfoUI>().SetOnPanel(IsPlaying);
    }
}
