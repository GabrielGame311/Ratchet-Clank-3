using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DiscBoltGet : MonoBehaviour
{

    public float DiscRotateSpeed;
    public Animator anime;
    public GameObject Disc_;
    GameObject DiscItem;
    public float TimelineTime;
    public int DisNumber;
    public GameObject Scene2;

    // Start is called before the first frame update
    void Start()
    {
        DiscItem = GameObject.FindGameObjectWithTag("Disc");
    }

    // Update is called once per frame
    void Update()
    {
        if(Disc_.activeSelf == true)
        {
            RunFalse();
        }


        DiscItem.transform.Rotate(+DiscRotateSpeed * Time.deltaTime, 0, 0);

    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Disc")
        {
            StartCoroutine(wait());
            RunFalse();
            Destroy(other.gameObject);
            GameObject.FindObjectOfType<ItemsList>().AddQarkGame(DisNumber);
        }
    }

    IEnumerator wait()
    {
        RunFalse();
        Disc_.SetActive(true);

        // anime.SetTrigger("Disc");

        InfoText.instance.InfoText_.SetActive(true);

        yield return new WaitForSeconds(TimelineTime);
        Disc_.SetActive(false);
        InfoText.instance.SetText("You got Qark-Comic Issue 2: " +
            "Arriba Amoeba!");
        InfoText.instance.Info2_.SetActive(true);
        yield return new WaitForSeconds(3);
        Scene2.SetActive(true);
    }

    public void RunFalse()
    {
        //GetComponent<Player>().anime.SetBool("Run", false);
    }

}
