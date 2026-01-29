using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackerGameUI : MonoBehaviour
{

    public GameObject GreenSpawn;
    public Transform Content_;
    bool Cantspawn = false;
    public List<GameObject> GreenPrefabs;
    public int Hits;
    Vector3 position;
    Quaternion[] Rotetion;
    Vector3[] pos;

    bool hasPlayed = false;


    // Start is called before the first frame update
    void Start()
    {
      position = GameObject.FindObjectOfType<ShootGame>().MainCamera_.transform.position;

        for(int i = 0; i < GameObject.FindObjectOfType<ShootGame>().Circles_.Length; i++)
        {
          Rotetion[i] = GameObject.FindObjectOfType<ShootGame>().Circles_[i].transform.rotation;
            pos[i] = GameObject.FindObjectOfType<ShootGame>().Circles_[i].transform.position;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Hits >= 8 && !hasPlayed)
        {
            Cantspawn = true;
            hasPlayed = true; // förhindra flera spelningar

            GameObject.FindObjectOfType<ShootGame>().PLaying.Play();
            StartCoroutine(wait());
        }
        else if (Hits < 8)
        {
            Cantspawn = false;
            hasPlayed = false; // återställ flaggan om man går tillbaka

            GameObject.FindObjectOfType<ShootGame>().MainCamera_.transform.position = position;

            for (int i = 0; i < GameObject.FindObjectOfType<ShootGame>().Circles_.Length; i++)
            {
                GameObject.FindObjectOfType<ShootGame>().Circles_[i].transform.rotation = Rotetion[i];
                GameObject.FindObjectOfType<ShootGame>().Circles_[i].transform.position = pos[i];
            }
        }
    }


    IEnumerator wait()
    {
        yield return new WaitForSeconds(2);
        
        GameObject.FindObjectOfType<ShootGame>().PLaying.Stop();
        Hits = 0;
        GameObject.FindObjectOfType<HackerGameEnable>().DisableHackerGame();
        foreach (GameObject pl in GreenPrefabs)
        {
            Destroy(pl);
        }
        GreenPrefabs.Clear();
    }

    public void HasHit()
    {
        if(Cantspawn == false)
        {
           GameObject pr = Instantiate(GreenSpawn, Content_.transform);
            GreenPrefabs.Add(pr);
            Hits++;
        }
    }
}
