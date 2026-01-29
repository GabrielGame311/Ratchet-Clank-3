using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class GadgeTronArmorUI : MonoBehaviour
{

    public Mesh[] MeshArmor;
    public int Armor;
    public SkinnedMeshRenderer Player_Mesh;
    public Material[] meshMaterial_Armor1, meshMaterial_Armor2;

    public TMP_Text CostPrice_Text;
    public TMP_Text Bolts_Text;

    public int[] ArmorBoltPrice;
    public GameObject Helmet;

    public Color CostPriceHave;
    public Color CostPriceDontHave;

    public int Bolts;


   


    // Start is called before the first frame update
    void Start()
    {
        Armor = 1;
        gameObject.SetActive(false);
    }


    

    // Update is called once per frame
    void Update()
    {


        Bolts = GameObject.FindObjectOfType<Bolts>().bolt;

        Bolts_Text.text = Bolts.ToString();

       

        if (Bolts >= ArmorBoltPrice[0])
        {

            
           
            if (Armor == 0)
            {
                CostPrice_Text.color = CostPriceHave;
                CostPrice_Text.text = "Cost: " + ArmorBoltPrice[0].ToString();
                SetArmor(0);
            }
        }
        else
        {
            if (Armor == 0)
            {
                CostPrice_Text.color = CostPriceDontHave;
                CostPrice_Text.text = "Cost: " + ArmorBoltPrice[0].ToString();
            }

            
        }
        if (Bolts >= ArmorBoltPrice[1])
        {
            
            

            if (Armor == 1)
            {
                CostPrice_Text.color = CostPriceHave;
                CostPrice_Text.text = "Cost: " + ArmorBoltPrice[1].ToString();
                SetArmor(1);

            }
        }
        else
        {
            if(Armor == 1)
            {
                CostPrice_Text.text = "Cost: " + ArmorBoltPrice[1].ToString();
                CostPrice_Text.color = CostPriceDontHave;
            }

        }
        if (Bolts >= ArmorBoltPrice[2])
        {
           
           
            if (Armor == 2)
            {
                CostPrice_Text.text = "Cost: " + ArmorBoltPrice[2].ToString();
                CostPrice_Text.color = CostPriceHave;
                SetArmor(2);
            }

        }
        else
        {   if (Armor == 2)
            {
                CostPrice_Text.text = "Cost: " + ArmorBoltPrice[2].ToString();
                CostPrice_Text.color = CostPriceDontHave;
            }
        }
        if (Bolts >= ArmorBoltPrice[3])
        {
            
           
            if (Armor == 3)
            {
                CostPrice_Text.color = CostPriceHave;
                CostPrice_Text.text = "Cost: " + ArmorBoltPrice[3].ToString();
                SetArmor(3);

            }
        }
        else
        {   if (Armor == 3)
            {
                CostPrice_Text.text = ArmorBoltPrice[3].ToString();
                CostPrice_Text.text = "Cost: " + CostPriceDontHave;
            }
        }

       
        
       
      

       
    }


    public void SkinsCurrent()
    {

    }

    public void SetArmor(int seting)
    {

    
        

        if (seting < 0 || seting >= meshMaterial_Armor1.Length ||
            seting >= meshMaterial_Armor2.Length ||
            seting >= MeshArmor.Length)
        {
            Debug.LogWarning("SetArmor index out of bounds!");
            return;
        }

        if (meshMaterial_Armor1[seting] == null ||
            meshMaterial_Armor2[seting] == null ||
            MeshArmor[seting] == null)
        {
            Debug.LogWarning("One or more armor assets are missing.");
            return;
        }
       
        // Prepare a new materials array if needed
        Material[] mats = Player_Mesh.materials;
        if (Armor > 0)
        {
            if (mats.Length < 4)
            {
                mats = new Material[4];
            }
        }
        else
        {
            if (mats.Length > 4)
            {

                Destroy(mats[0]);
            }
        }
        if (Armor == 2)
        {
            if (mats.Length < 5)
            {
                mats = new Material[5];
            }


            mats[0] = meshMaterial_Armor1[seting];
            mats[1] = meshMaterial_Armor2[seting];
            mats[2] = meshMaterial_Armor2[seting];
            mats[3] = meshMaterial_Armor1[seting];
            mats[4] = meshMaterial_Armor1[seting];
        }
        else
        {
            mats[0] = meshMaterial_Armor1[seting];
            mats[1] = meshMaterial_Armor2[seting];
            mats[2] = meshMaterial_Armor1[seting];
            mats[3] = meshMaterial_Armor2[seting];
        }
        if (Armor == 3)
        {
            Helmet.SetActive(false);
        }
        else
        {
            Helmet.SetActive(true);
        }
        // Assign armor materials


        Player_Mesh.materials = mats;

        // Assign new mesh
        Player_Mesh.sharedMesh = MeshArmor[seting];
    }
}
