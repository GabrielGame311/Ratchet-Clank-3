using UnityEngine;

public class TriggerRangers : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {


            foreach(GalacticRangerGame gm in GameObject.FindObjectsOfType<GalacticRangerGame>())
            {
                gm.StartRun = true;
            }
        }
    }
}
