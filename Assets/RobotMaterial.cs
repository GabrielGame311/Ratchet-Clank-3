using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotMaterial : MonoBehaviour
{
    public GameObject[] Robots_;
    public Material RobotMaterial_;

    public bool IsRobot;

    // Start is called before the first frame update
    void Start()
    {
        Robots_ = GameObject.FindGameObjectsWithTag("Robot");
    }

    // Update is called once per frame
    void Update()
    {
        if (IsRobot)
        {
            foreach (GameObject rb in Robots_)
            {
                Renderer renderer = rb.GetComponent<Renderer>();
                if (renderer != null)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        mat.mainTexture = RobotMaterial_.mainTexture;
                    }
                }
                else
                {
                    Debug.LogWarning($"Renderer component missing on {rb.name}");
                }
            }

            // Reset IsRobot to false to avoid setting textures every frame
            
        }
       
    }
}
