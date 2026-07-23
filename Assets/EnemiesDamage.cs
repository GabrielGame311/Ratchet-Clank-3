using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX; // Kom ihåg att importera denna!

public class EnemiesDamage : MonoBehaviour
{
    [Header("Laser Settings")]
    public int laserDamage = 10;       // Skada som lasern gör
    public float laserRange = 20f;     // Hur långt lasern når
    public LayerMask hitLayers;        // Kom ihåg att bocka i både Player och Enemie-lagren i Unity!
    public float laserDuration = 0.1f; // Hur länge lasereffekten ska vara aktiv

    [Header("VFX Graph Settings")]
    private VisualEffect vfxLaser;     // Referens till din Visual Effect-komponent
    
    // Sträng-namnen måste matcha exakt det du har döpt dem till i din VFX Graph (Blackboard)
    public string startPosName = "LaserStart";
    public string endPosName = "LaserEnd";

    private float laserTimer;         
    private bool laserHitTarget = false; 

    void Start()
    {
        vfxLaser = GetComponent<VisualEffect>(); 
        laserTimer = laserDuration;

        if (vfxLaser != null)
        {
            vfxLaser.Play(); // Starta eller aktivera VFX-effekten
        }
    }

    void Update()
    {
        ShootLaser();
    }

    void ShootLaser()
    {
        RaycastHit hit;
        laserHitTarget = false; 

        // Sätt alltid startpunkten till vapnets/skriptets position
        Vector3 startPosition = transform.position;
        Vector3 endPosition;

        // Skjut lasern rakt framåt med Raycast
        if (Physics.Raycast(startPosition, transform.forward, out hit, laserRange, hitLayers))
        {
            // Träffpunkt blir laserns slutdestination
            endPosition = hit.point;

            // Se till att lasern inte träffar den fiende som faktiskt skjuter den!
            if (hit.transform.root != transform.root)
            {
                // Skicka positionerna direkt till din VFX Graph!
                UpdateVFXPositions(startPosition, endPosition);

                // 1. TRÄFFAR SPELAREN
                if (hit.collider.CompareTag("Player") && !laserHitTarget)
                {
                    Player playerScript = hit.collider.GetComponent<Player>();
                    if (playerScript != null)
                    {
                        playerScript.TakeDamage(laserDamage);
                        laserHitTarget = true;
                        
                        // Ta bort skadeskriptet så den inte skadar mer (behåller VFX-objektet vid liv)
                        Destroy(GetComponent<EnemiesDamage>()); 
                    }
                }
                // 2. TRÄFFAR EN FIENDE
                else if (hit.collider.CompareTag("Enemie") && !laserHitTarget)
                {
                    EnemiesHealth enemyHealth = hit.collider.GetComponent<EnemiesHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(laserDamage);
                        laserHitTarget = true;
                        
                        Destroy(GetComponent<EnemiesDamage>()); 
                    }
                }
                else if(hit.collider.CompareTag("RangerCover") && !laserHitTarget)
                {
                    RangerCover rangerCover = hit.collider.GetComponent<RangerCover>();
                    if (rangerCover != null)
                    {
                        rangerCover.TakeDamage(laserDamage);
                        laserHitTarget = true;
                        
                        Destroy(GetComponent<EnemiesDamage>()); 
                    }
                }
                 else if(hit.collider.CompareTag("Ranger") && !laserHitTarget)
                {
                    RangerHealth rangerCover = hit.collider.GetComponent<RangerHealth>();
                    if (rangerCover != null)
                    {
                        rangerCover.TakeDamage(laserDamage);
                        laserHitTarget = true;
                        
                        Destroy(GetComponent<EnemiesDamage>()); 
                    }
                }
            }
        }
        else
        {
            // Om vi inte träffar något, sätt slutpunkten till max räckvidd rakt framåt
            endPosition = startPosition + (transform.forward * laserRange);
            UpdateVFXPositions(startPosition, endPosition);
        }

        // Timer för att stänga av VFX-lasern efter dess duration
        laserTimer -= Time.deltaTime;
        if (laserTimer <= 0)
        {
            if (vfxLaser != null)
            {
                vfxLaser.Stop(); // Stoppar partikelutsläppet i VFX Graphen
            }
        }
    }

    // Hjälpmetod för att säkert skicka koordinater till grafikkortet (VFX Graph)
    void UpdateVFXPositions(Vector3 start, Vector3 end)
    {
        if (vfxLaser != null)
        {
            vfxLaser.SetVector3(startPosName, start);
            vfxLaser.SetVector3(endPosName, end);
        }
    }
}