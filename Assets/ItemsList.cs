using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsList : MonoBehaviour
{

    public List<GameObject> ItemsList_;

    public Image[] img;



    // Start is called before the first frame update
    void Start()
    {
        foreach(Image gm in img)
        {
           

           
            gm.GetComponent<Image>().enabled = false;
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }




    public void AddQarkGame(int item)
    {
        
       

            ItemsList_[item].GetComponent<Button>().interactable = true;

            img[item].GetComponentInChildren<Image>().enabled = true;
        
    }
}
