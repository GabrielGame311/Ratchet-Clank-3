using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : MonoBehaviour
{
    public GameObject sight;
   public bool IsSight = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(IsSight == true)
        {
            sight.transform.LookAt(GameObject.FindGameObjectWithTag("Player").GetComponent<GameObject>().transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            sight.SetActive(true);
            IsSight = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            sight.SetActive(false);
            IsSight = false;
        }
    }
}
