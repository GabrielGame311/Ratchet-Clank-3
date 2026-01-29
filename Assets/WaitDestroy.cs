using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitDestroy : MonoBehaviour
{
    public float WaitTime;
    
    
    // Start is called before the first frame update
    void Start()
    {
        destroy();
    }

   


    public void destroy()
    {
        StartCoroutine(wait());
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(WaitTime);

        Destroy(gameObject);
    }
}
