using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class GalacticRangerGame : MonoBehaviour
{

    public Transform Point;
    public GameObject Granade;
    private NavMeshAgent agent;

    public Animator HeadAnime;
    public Animator FootAnime;

    public float SpeedRun;

    public Transform PointRun;

    public float TimeAnime;

    public bool StartAnime;

    GalacticRangers instance;

    public float ForceBall = 360;
    public float MoveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        instance = GetComponent<GalacticRangers>();
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            agent.isStopped = true; // Stannar agenten
        }

        if (StartAnime)
        {
            instance.enabled = false;
          

          
            StartCoroutine(runing());



            TimeAnime -= Time.deltaTime;
            if(TimeAnime < 0)
            {
                HeadAnime.SetBool("Run", false);
                FootAnime.SetBool("Run", false);
                TimeAnime = 0;
                StartAnime = false;

            } 

        }
        else
        {
            instance.enabled = true;
        }
    }
    public void MoveToDestination()
    {
        if (PointRun != null)
        {
            MoveTo(PointRun.transform.position);
        }
    }

    public void MoveTo(Vector3 destination)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(destination, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log("Moving to: " + hit.position);
        }
        else
        {
            Debug.LogWarning("Destination is not on NavMesh!");
        }
    }


    IEnumerator runing()
    {
        HeadAnime.SetTrigger("Throw");

        FootAnime.SetTrigger("Throw");
        yield return new WaitForSeconds(1);
        HeadAnime.SetBool("Run", true);
        FootAnime.SetBool("Run", true);

        transform.position = Vector3.MoveTowards(transform.position, PointRun.transform.position, MoveSpeed * Time.deltaTime);

        MoveToDestination();


        

    }

    public void ThrowGranade()
    {

        GameObject prefab = Instantiate(Granade, Point.transform.position, Point.transform.rotation);
        Vector3 launchVelocity = Point.transform.forward * ForceBall + Vector3.up * (ForceBall / 2f);
        prefab.GetComponent<Rigidbody>().velocity = launchVelocity;

        // Aktivera gravitation så att den faller naturligt
        prefab.GetComponent<Rigidbody>().useGravity = true;


    }
}
