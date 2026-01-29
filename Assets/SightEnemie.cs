using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightEnemie : MonoBehaviour
{

    public float maxDistance = 10f; // Maximum distance to detect enemies
    Vector3 startpos;
    public Transform SightTransform3D;
    private bool isEnemyVisible = false;
    public Transform enemyTransform;
    public LayerMask TargetLayer;
    private void Start()
    {
        startpos = SightTransform3D.transform.position;
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
