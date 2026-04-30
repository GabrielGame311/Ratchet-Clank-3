using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Bolts : MonoBehaviour
{

    public int bolt;
    public int BoltCount;
    public TMP_Text Bolts_Text;
    public TMP_Text BoltsCounting_Text;
    public float CountTime = 2;
    float starttime;
    public static Bolts Bolt;

    public float BonusUITime = 30;
    float StartTimeBonus;
    public float EndBonusTime = 8;
    float StartEndBonusTime;
    public int scoreMultiplier = 1;
    public Animator animeBonus;
    public AudioClip soundfx;
    public bool IsBonus = false;
    AudioSource sound;
    private bool hasPlayedSound = false;
    public TMP_Text Bonus_Text;
    void Start()
    {
        //bolt = PlayerPrefs.GetInt("Bolt", 0);
        StartEndBonusTime = EndBonusTime;
        sound = GetComponent<AudioSource>();
        StartTimeBonus = BonusUITime;
        Bolt = GetComponent<Bolts>();
        //bolt = 0;

        BoltsCounting_Text.text = "";

        starttime = CountTime;
    }

    // Update is called once per frame
    void Update()
    {
        Bolts_Text.text = bolt.ToString();
        //PlayerPrefs.SetInt("Bolt", bolt);
       // PlayerPrefs.Save();
        if (BoltCount > 0)
        {
            CountTime -= Time.deltaTime;
           
            BoltsCounting_Text.text = "+" + BoltCount.ToString();
        }

        if (CountTime <= 0)
        {
            
            StartCoroutine(AddBoltsOverTime(BoltCount, 1));
            CountTime = starttime;
           // bolt += BoltCount * scoreMultiplier;
            BoltCount = 0;
            BoltsCounting_Text.text = "";
        }

        if(IsBonus)
        {

            animeBonus.gameObject.SetActive(true);
            Bonus_Text.text = "X" + scoreMultiplier.ToString();
            if(BonusUITime < 0)
            {
                
                animeBonus.SetTrigger("Closed");

               

                if (EndBonusTime < 0)
                {

                    BonusUITime = StartTimeBonus;
                    EndBonusTime = StartEndBonusTime;
                    scoreMultiplier = 1;
                    IsBonus = false;
                    animeBonus.gameObject.SetActive(false);
                   

                }
                else
                {
                    EndBonusTime -= Time.deltaTime;
                }
            }
            else
            {
                BonusUITime -= Time.deltaTime;
            }


        }

    }

    public void Playsound()
    {
        sound.PlayOneShot(soundfx);
    }


    IEnumerator AddBoltsOverTime(int boltCount, float duration)
    {
        float elapsed = 0.0f;
        float increment = (float)boltCount / duration;
        int startBolt = bolt;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bolt = (int)(startBolt + increment * elapsed);
            Bolts_Text.text = bolt.ToString();

            yield return null;
        }

        int endBolt = startBolt + boltCount;
        bolt = endBolt;
        Bolts_Text.text = bolt.ToString();
        BoltsCounting_Text.text = "";

        BoltCount = 0;
       

    }







}
