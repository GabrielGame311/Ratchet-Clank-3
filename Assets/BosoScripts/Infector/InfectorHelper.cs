using System;
using System.Collections;
using UnityEngine;

public class InfectorHelper : MonoBehaviour
{
    public float InfectionTime = 12.0f;


    private LayerMask _cachedMask;
    private MobyTargetting _targetting;

    private void Start()
    {
        BeginInfection();
    }

    public void BeginInfection()
    {

        if (TryGetComponent(out  _targetting))
        {
            _cachedMask = _targetting.EnemyMask;
            _targetting.EnemyMask = LayerMask.GetMask("Enemie");
            StartCoroutine(InfectionTick());
        }
        else
        {
            Debug.LogWarning("No modern targetting component, cant use infector on this.");
        }
    }

    private IEnumerator InfectionTick()
    {
        float elapsed = 0.0f;
        while (true)
        {
            elapsed += Time.deltaTime;
            if(elapsed >= InfectionTime)
            {
                _targetting.EnemyMask = _cachedMask;
                Destroy(this);
            }
                yield return null;
            
        }
    }
}
