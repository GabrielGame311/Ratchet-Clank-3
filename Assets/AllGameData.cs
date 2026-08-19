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


    [Header("Checkpoint Data")]
    public Vector3 lastCheckpointPos;
    public Quaternion lastCheckpointRot;
    public bool hasCheckpoint = false;




    void Start()
    {

        Instance = this;
        CurrentSaveSlot = LoadMapName.NextSaveSlot;

        


        InitializeGame();
        
        
        
    }

    /// <summary>
    /// Sätter ny checkpoint-position
    /// </summary>
    public void SetCheckpoint(Vector3 pos, Quaternion rot)
    {
        lastCheckpointPos = pos;
        lastCheckpointRot = rot;
        hasCheckpoint = true;
    }

    /// <summary>
    /// Anropa denna när spelaren dör för att spawna om vid senast sparade checkpoint
    /// </summary>
    public void RespawnPlayer()
    {
        if (Player_ == null) return;

        if (hasCheckpoint)
        {
            // Inaktivera CharacterController tillfälligt för att tillåta förflyttning
            CharacterController cc = Player_.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            Player_.transform.position = lastCheckpointPos;
            Player_.transform.rotation = lastCheckpointRot;

            if (cc != null) cc.enabled = true;

            // Återaktivera spelarkontrollen och nollställ hälsa/tillstånd här vid behov
            EnablePlayerDo();
        }
        else
        {
            // A new game has no checkpoint yet. Keep the scene start position.
            return;
        }
    }


    private void InitializeGame()
    {
        

        if (LoadingScene.instance != null && !string.IsNullOrEmpty(LoadingScene.instance.LoadMap))
        {
            SavedMap = LoadingScene.instance.LoadMap;
        }
        CurrentMapInt = SceneManager.GetActiveScene().buildIndex;
        if (!SaveUtility.TryReadSave(CurrentSaveSlot, out SaveData data))
        {
            Debug.LogWarning("Save not found in slot " + CurrentSaveSlot);
            return;
        }

        if (IsSave == false)
        {
            
            SaveSystem.LoadGame(CurrentSaveSlot, false);
            RespawnPlayer();
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
        Armor = setting;

        // 1. S�kerhetskoll f�r index (s� vi inte g�r utanf�r arrayernas storlek)
        if (setting < 0 ||
            setting >= meshMaterial_Armor1.Length ||
            setting >= meshMaterial_Armor2.Length ||
            setting >= MeshArmor.Length)
        {
            Debug.LogWarning($"SetArmor index {setting} �r out of bounds!");
            return;
        }

        // 2. Kolla s� att inte objekten i arrayen �r tomma (Null)
        if (meshMaterial_Armor1[setting] == null ||
            meshMaterial_Armor2[setting] == null ||
            MeshArmor[setting] == null)
        {
            Debug.LogWarning($"N�gon tillg�ng saknas i inspektorn f�r index {setting}.");
            return;
        }

        Material[] mats;

        // 3. Best�m hur m�nga material den aktuella rustningen har
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
            // H�R HANTERAS NIV� 4: Den f�r ocks� 5 material (eller �ndra till 6 om du la till �nnu ett)
            mats = new Material[5];
            mats[0] = meshMaterial_Armor1[setting];
            mats[1] = meshMaterial_Armor2[setting];
            mats[2] = meshMaterial_Armor1[setting];
            mats[3] = meshMaterial_Armor2[setting];
            mats[4] = meshMaterial_Armor1[setting]; // Ditt nya 5:e material
        }
        else // G�ller f�r 0, 1 och 2 (som bara har 4 material)
        {
            mats = new Material[4];
            mats[0] = meshMaterial_Armor1[setting];
            mats[1] = meshMaterial_Armor2[setting];
            mats[2] = meshMaterial_Armor1[setting];
            mats[3] = meshMaterial_Armor2[setting];
        }

        // 4. Hantera hj�lmen
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
