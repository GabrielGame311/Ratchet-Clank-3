using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelikopterController : MonoBehaviour
{

    public Animator anime;
    Animator skinanime;
    public static HelikopterController Instance;


    // Start is called before the first frame update
    void Start()
    {

        Instance = this;
        skinanime = GameObject.FindGameObjectWithTag("Ratchet").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void StartHelikopter()
    {
        anime.SetBool("Start", true);
        skinanime.SetBool("Fly", true);
    }

    public void CancelHelikopter()
    {
        anime.SetBool("Start", false);
        skinanime.SetBool("Fly", false);
    }
}
