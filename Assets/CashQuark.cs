using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashQuark : MonoBehaviour
{

    public int Cash_;

    public GameObject Particle;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject.FindObjectOfType<CashText>().Cash_ = Cash_;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Cash")
        {
            Cash_++;

            GameObject pl = Instantiate(Particle, other.transform.position, other.transform.rotation);
            Destroy(pl, 3);
            Destroy(other.gameObject);
        }
        else if (other.tag == "RedToken")
        {
            Cash_ += 10;
            GameObject pl = Instantiate(Particle, other.transform.position, other.transform.rotation);
            Destroy(pl, 3);
            Destroy(other.gameObject);
        }
    }
}
