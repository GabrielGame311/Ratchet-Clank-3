using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RangerTrigger : MonoBehaviour
{

    public string SceneLoad;
    public static bool IsTrigger = false;


    public static RangerTrigger RangerTrigger_;
    // Start is called before the first frame update
    void Start()
    {
        RangerTrigger_ = GetComponent<RangerTrigger>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            IsTrigger = true;
            SceneManager.LoadScene(SceneLoad);
        }
    }

   

}
