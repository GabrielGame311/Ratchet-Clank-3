using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightEnemie : MonoBehaviour
{

    public float maxDistance = 10f; // Maximum distance to detect enemies
    Vector3 startpos;
    public Transform SightTransform3D;
    public bool isEnemyVisible = false;
    public Transform enemyTransform;
    public LayerMask TargetLayer;
    public float verticalOffset = 1.5f;
    public static SightEnemie Instance;
    private void Start()
    {
        startpos = SightTransform3D.transform.position;
        Instance = this;
    }

    private void Update()
    {
        DetectEnemies();
        UpdateSightUIPosition();
    }

    private void DetectEnemies()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, TargetLayer))
        {
            if (hit.collider.tag == "Sight")
            {
                // An enemy is within sight.
                enemyTransform = hit.transform;
                SightTransform3D = hit.transform;
                Vector3 adjustedHitPoint = hit.point + Vector3.up * verticalOffset;
                isEnemyVisible = true;
            }
           
            // An enemy is within sight.
            enemyTransform = hit.transform;
            SightTransform3D = hit.transform;
           
            isEnemyVisible = true;

        }
        else
        {
            // No objects hit within maxDistance.
            isEnemyVisible = false;
        }
    }

    private void UpdateSightUIPosition()
    {
        if (isEnemyVisible && enemyTransform != null)
        {
            Vector3 enemyScreenPos = Camera.main.WorldToScreenPoint(enemyTransform.position);
            SightUI.SightUI_.Sight.transform.position = enemyScreenPos;
            SightUI.SightUI_.Sight.SetActive(true);
        }
        else
        {
            SightUI.SightUI_.Sight.SetActive(false);
        }

      
       
    }
}
