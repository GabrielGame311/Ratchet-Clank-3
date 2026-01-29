using UnityEngine;

public class AimTarget3D : MonoBehaviour
{
    public Transform weaponTransform; // Referens till vapnets transform
    public float reticleDistance = 10f; // Standardavstånd från vapnet där siktet ska visas
    public Vector3 reticleScale = Vector3.one * 2f; // Standardstorlek på siktet
    public LayerMask targetLayers; // Vilka lager (layers) siktet ska träffa (t.ex. fiender, väggar, mark)
    public float groundOffset = 0.1f; // Liten offset från marken för att undvika att siktet sjunker in
    public float minSiktAvstånd = 1f; // Minsta avstånd från vapnet till siktet
    public float maxSiktAvstånd = 50f; // Maximalt avstånd som siktet kan hamna på
    Transform player_;
    Quaternion StartRot;

    private void Start()
    {
        player_ = GameObject.FindGameObjectWithTag("Player").transform;
        StartRot = transform.rotation;
    }


    void Update()
    {
        // Skapa en Raycast från vapnets position i dess framåtriktning
        Ray ray = new Ray(weaponTransform.position, weaponTransform.forward);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, maxSiktAvstånd, targetLayers))// Använd targetLayers för att filtrera träffar
        {
            // Kontrollera avståndet till träffpunkten
            float distanceToHit = Vector3.Distance(weaponTransform.position, hit.point);

            if (distanceToHit < minSiktAvstånd) // Om träffpunkten är för nära, justera avståndet
            {
                transform.position = weaponTransform.position + weaponTransform.forward * minSiktAvstånd;
            }
            else if (distanceToHit > maxSiktAvstånd) // Om träffpunkten är för långt bort, justera avståndet
            {
                transform.position = weaponTransform.position + weaponTransform.forward * maxSiktAvstånd;
            }
            else
            {
                // Placera siktet på träffpunkten om avståndet är inom acceptabelt intervall
                transform.position = hit.point;
                transform.LookAt(player_.transform);
            }

            Vector3 startpos = transform.position;
            // Håll siktet upprätt (ingen rotation om det träffar något)
            if (hit.collider.CompareTag("Enemie"))
            {
                transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Rotera 90 grader



                transform.position = hit.transform.position;
            }
            else
            {
                transform.LookAt(player_); // Behåll standardrotation
                transform.position = startpos;
            }
        }
        else
        {
            // Om ingen träff, placera siktet på marken och rotera 90 grader
            transform.position = weaponTransform.position + weaponTransform.forward * maxSiktAvstånd;
            PlaceReticleOnGround();
        }

        // Håll skalan konstant och förstorad
        transform.localScale = reticleScale;
    }

    void PlaceReticleOnGround()
    {
        // Skicka en Raycast nedåt (längs Y-axeln) från en position framför vapnet för att hitta marken
        Vector3 startPosition = weaponTransform.position + weaponTransform.forward * reticleDistance;
        Ray downRay = new Ray(startPosition, Vector3.down);
        RaycastHit groundHit;

        if (Physics.Raycast(downRay, out groundHit, Mathf.Infinity, targetLayers))
        {
            // Placera siktet på marken med en liten offset uppåt för att undvika att det sjunker in
            transform.position = groundHit.point + Vector3.up * groundOffset;

            // Rotera siktet 90 grader runt X-axeln (så det ligger plant på marken)
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            // Om ingen mark hittas, placera siktet på ett fallback-avstånd under startpositionen
            transform.position = startPosition + Vector3.down * 1f;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }









}
