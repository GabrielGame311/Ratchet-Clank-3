using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemieSpawn : MonoBehaviour
{
    public float Speed;

    public Transform PositionSpawn;
    public float Distance;

    GameObject player_;

    bool isPosition = false;

    // Start is called before the first frame update
    void Start()
    {
        player_ = GameObject.FindGameObjectWithTag("Player");

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        // Iterate through all scripts and disable them
        foreach (MonoBehaviour script in scripts)
        {
            // Ensure we don't disable the DisableAllScripts script itself
            if (script != this)
            {
                script.enabled = false;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

        float dis = Vector3.Distance(transform.position, player_.transform.position);



        if(isPosition == false)
        {
            if (dis < Distance)
            {
                transform.position = Vector3.MoveTowards(transform.position, PositionSpawn.transform.position, Speed * Time.deltaTime);

                if (transform.position == PositionSpawn.position)
                {
                    MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
                    // Iterate through all scripts and disable them
                    foreach (MonoBehaviour script in scripts)
                    {
                        // Ensure we don't disable the DisableAllScripts script itself
                        if (script != this)
                        {
                            script.enabled = true;
                        }
                    }
                    isPosition = true;
                }

            }
        }


    }
}
