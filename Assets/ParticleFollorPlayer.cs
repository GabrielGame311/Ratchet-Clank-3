using UnityEngine;

public class ParticleFollorPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created



    public bool IsFollow = false;
    GameObject player;

    public float Distance;
    public float SpeedMove;
    public float HPGet;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if(IsFollow)
        {
            float dis = Vector3.Distance(transform.position, player.transform.position);


            if(dis < Distance)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, SpeedMove * Time.deltaTime);



            }
            
        }

        if (GameObject.FindObjectOfType<Player>().currentHealth < GameObject.FindObjectOfType<Player>().maxHealth)
        {
            IsFollow = true;
        }
    }

   

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(IsFollow)
            {
                Player player = other.gameObject.GetComponent<Player>();

                // Lägg till HP men begränsa resultatet mellan 0 och 100
                player.currentHealth = Mathf.Clamp(player.currentHealth + (int)HPGet, 0, 100);

                
                Destroy(gameObject);
            }
          
        }
    }
}
