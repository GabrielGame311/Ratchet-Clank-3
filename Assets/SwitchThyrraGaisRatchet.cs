using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchThyrraGaisRatchet : MonoBehaviour
{

    public GameObject[] SwitchPlayer;

    public KeyCode InputSwitch;
    public int ItsPlayer;
    public Animator ThyrraAnime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(InputSwitch))
        {
            SwitchPlayer[1].SetActive(false);
            SetPlayer();
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            ThyrraAnime.SetTrigger("Talking");
        }

    }

    public void SetPlayer()
    {

        if(ItsPlayer == 0)
        {
            SwitchPlayer[0].SetActive(false);
            ItsPlayer++;
            SwitchPlayer[ItsPlayer].SetActive(true);
        }
        else if(ItsPlayer == 1)
        {
            SwitchPlayer[1].SetActive(false);
            ItsPlayer--;
            SwitchPlayer[ItsPlayer].SetActive(true);
        }

    }
}
