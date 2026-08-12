using UnityEngine;

public class TorretGun : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject ball;
    public GameObject torretv5;

    [Header("Settings & References")]
    public Transform ballpos;
    public float bulletSpeed = 10f;
    public Animator anime;

    private WeaponAmmos weaponAmmo;
    private WeaponsUI weaponUI;

    private void Awake()
    {
        weaponAmmo = GetComponent<WeaponAmmos>();
        weaponUI = GetComponent<WeaponsUI>();
    }

    private void Start()
    {
        // Försök hitta Animator om den inte redan tilldelats i Inspector
        if (anime == null)
        {
            GameObject ratchet = GameObject.FindGameObjectWithTag("Ratchet");
            if (ratchet != null)
            {
                anime = ratchet.GetComponent<Animator>();
            }
        }
    }

    private void Update()
    {
        if (weaponAmmo == null || !weaponAmmo.enabled) return;

        // Om vi inte kör via mobil/iOS-kontroller
        if (IOSController.IosController_ == null)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Fire();
            }
        }
    }

    public void Fire()
    {
        if (weaponAmmo != null && weaponAmmo.Ammo > 0 && weaponAmmo.TryShoot())
        {
            TriggerThrowAnimation();
        }
    }

    public void Shoots()
    {
        TriggerThrowAnimation();
    }

    private void TriggerThrowAnimation()
    {
        if (anime != null)
        {
            anime.SetTrigger("Throw");
        }
    }

    /// <summary>
    /// Anropas från Animation Event på kast-animationen
    /// </summary>
    public void ThrowBalls()
    {
        if (ballpos == null) return;

        int currentLevel = weaponUI != null ? weaponUI.level : 1;
        GameObject prefabToSpawn = (currentLevel >= 5 && torretv5 != null) ? torretv5 : ball;

        if (prefabToSpawn == null) return;

        // Skapa boll/torn-projektil
        GameObject spawnedBall = Instantiate(prefabToSpawn, ballpos.position, ballpos.rotation);

        // Skjut iväg projektilen med fysik
        if (spawnedBall.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(ballpos.forward * bulletSpeed, ForceMode.Impulse);
        }

        // Tilldela referens om det är en Level 5-torret
        if (currentLevel >= 5 && spawnedBall.TryGetComponent<Ball>(out Ball ballScript))
        {
            ballScript.torret = torretv5;
        }
    }
}