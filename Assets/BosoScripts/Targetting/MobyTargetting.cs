using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MobyTargetting : MonoBehaviour
{
   public GameObject CurrentTarget;

   public float TargetingRange = 10f;
   public float TargettingRate = 2.0f; // Every x seconds scan
   public LayerMask EnemyMask;



   private void Awake()
   {
   }

   private void Start()
   {
      StartCoroutine(ScanForTargets());
   }

   private IEnumerator ScanForTargets()
   {
      
      while (true)
      {
         yield return new WaitForSeconds(TargettingRate);
         Collider[] enemies = Physics.OverlapSphere(transform.position, TargetingRange, EnemyMask);
         // Sort from closest to farthest
         List<Collider> sortedEnemies = enemies
            .OrderBy(enemy => Vector3.Distance(transform.position, enemy.transform.position))
            .ToList();

         sortedEnemies.RemoveAll(x => x.gameObject == gameObject || x.gameObject.transform.IsChildOf(transform) == true);
         
         if (sortedEnemies.Count > 0)
         {
            CurrentTarget = sortedEnemies[0].gameObject;
         }
      }

      yield return null;
   }

}
