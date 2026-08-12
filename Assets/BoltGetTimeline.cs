using UnityEngine;

public class BoltGetTimeline : MonoBehaviour
{

    public GameObject BoltPlayer;

    public float TimeBolt;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "BoltGet")
        {
            BoltPlayer.SetActive(true);
            WeaponSwitcher.Instance.DeactivateWeapons();
            RatchetController.RatchetController_.CanMove= false;
            StartCoroutine("DisableBolt");
            Destroy(other.gameObject);
        }
    }


    System.Collections.IEnumerator DisableBolt()
    {
        yield return new WaitForSeconds(TimeBolt);
        RatchetController.RatchetController_.CanMove = true;
        BoltPlayer.SetActive(false);
        WeaponSwitcher.Instance.ActivateWeapons();
        
    }
}
