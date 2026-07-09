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
    AllGameData[] data;
    public int CurrentSaveSlot = 0;
    public bool IsSave = false;
    public bool IsPlayer = false;
    void Start()
    {

        if(!IsPlayer)
        {
            data = FindObjectsOfType<AllGameData>();


            foreach (AllGameData pl in data)
            {
                Armor = pl.Armor;
            }
        }

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

        if (Armor == 4)
        {
            SetArmor(4);

        }







    }


    public void SetArmor(int setting)
    {
        // 1. Säkerhetskoll för index (så vi inte går utanför arrayernas storlek)
        if (setting < 0 ||
            setting >= meshMaterial_Armor1.Length ||
            setting >= meshMaterial_Armor2.Length ||
            setting >= MeshArmor.Length)
        {
            Debug.LogWarning($"SetArmor index {setting} är out of bounds!");
            return;
        }

        // 2. Kolla så att inte objekten i arrayen är tomma (Null)
        if (meshMaterial_Armor1[setting] == null ||
            meshMaterial_Armor2[setting] == null ||
            MeshArmor[setting] == null)
        {
            Debug.LogWarning($"Någon tillgång saknas i inspektorn för index {setting}.");
            return;
        }

        Material[] mats;

        // 3. Bestäm hur många material den aktuella rustningen har
        if (setting == 3)
        {
            mats = new Material[5];
            mats[0] = meshMaterial_Armor1[setting];
            mats[1] = meshMaterial_Armor2[setting];
            mats[2] = meshMaterial_Armor2[setting];
            mats[3] = meshMaterial_Armor1[setting];
            mats[4] = meshMaterial_Armor1[setting];
        }
        else if (setting == 4)
        {
            // HÄR HANTERAS NIVÅ 4: Den får också 5 material (eller ändra till 6 om du la till ännu ett)
            mats = new Material[5];
            mats[0] = meshMaterial_Armor1[setting];
            mats[1] = meshMaterial_Armor2[setting];
            mats[2] = meshMaterial_Armor1[setting];
            mats[3] = meshMaterial_Armor2[setting];
            mats[4] = meshMaterial_Armor1[setting]; // Ditt nya 5:e material
        }
        else // Gäller för 0, 1 och 2 (som bara har 4 material)
        {
            mats = new Material[4];
            mats[0] = meshMaterial_Armor1[setting];
            mats[1] = meshMaterial_Armor2[setting];
            mats[2] = meshMaterial_Armor1[setting];
            mats[3] = meshMaterial_Armor2[setting];
        }

        // 4. Hantera hjälmen
        if (setting == 4)
        {
            Helmet.SetActive(false);
        }
        else
        {
            Helmet.SetActive(true);
        }

        // 5. Applicera den nya meshen och de nya materialen
        Player_Mesh.materials = mats;
        Player_Mesh.sharedMesh = MeshArmor[setting];
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
