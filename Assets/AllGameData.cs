using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class AllGameData : MonoBehaviour
{
    public GameObject[] EnableDisableScripts; // Array of GameObjects whose scripts you want to enable/disable
    public GameObject Player_;
    public static AllGameData Instance;

    public SkinnedMeshRenderer Player_Mesh;

    public GameObject Helmet;
    public Mesh[] MeshArmor;
    public Material[] meshMaterial_Armor1, meshMaterial_Armor2;
    public string SavedMap;
    public int StartMapInt;
    public int CurrentMapInt;
    public Sprite[] ImageMap;
    
    public int Armor;

    public int CurrentSaveSlot = 0;
    public bool IsSave = false;

    void Start()
    {



        Instance = this;

        


        InitializeGame();
        
        
        
    }

    private void InitializeGame()
    {
        

        SavedMap = LoadingScene.instance.LoadMap;
        CurrentMapInt = SceneManager.GetActiveScene().buildIndex;

        string path = SaveUtility.GetSavePath(CurrentSaveSlot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save not found in slot " + CurrentSaveSlot);
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (IsSave == false)
        {
            
            SaveSystem.LoadGame(CurrentSaveSlot, false);
            SaveSystem.SaveGame(CurrentSaveSlot);
            SaveUI saveUI = GameObject.FindObjectOfType<SaveUI>();
            if (saveUI != null)
            {
                saveUI.ShowSavingMessage();
            }
        }

        Debug.Log($"Loading game from slot {CurrentSaveSlot}");
    }




    private void Update()
    {





        
        
            if (Armor == 0)
            {

                SetArmor(0);
            }
        
        
        

            if (Armor == 1)
            {
                SetArmor(1);

            }
        
       
            if (Armor == 2)
            {
                SetArmor(2);
            }
       
        
            if (Armor == 3)
            {
                SetArmor(3);

            }
        


       
       
       

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
        if(Armor > 0)
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
        if(Armor == 2)
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
        if(Armor == 3)
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




    public void DisablePlayerDo()
    {
        foreach (GameObject obj in EnableDisableScripts)
        {
            if (obj != null)
            {
                MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour script in scripts)
                {
                    script.enabled = false;
                   
                }
            }
        }
        Player_.GetComponent<RatchetController>().CanMove = false;
    }

    public void EnablePlayerDo()
    {
        foreach (GameObject obj in EnableDisableScripts)
        {
            if (obj != null)
            {
                MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour script in scripts)
                {
                    script.enabled = true;
                    
                }
            }
        }
        Player_.GetComponent<RatchetController>().CanMove = true;
    }
}
