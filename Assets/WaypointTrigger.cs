using UnityEngine;

public class WaypointTrigger : MonoBehaviour
{






    public enum TriggerType { StopNPC, ResumeNPC}
    public TriggerType type = TriggerType.StopNPC;

    private void OnTriggerEnter(Collider other)
    {
        // Om det är StopRanger: Triggas när gubben själv kliver i den
        if (type == TriggerType.StopNPC && other.CompareTag("NPC"))
        {
            Sgit.Instance.StopAtLocation();
        }
        
        // Om det är ResumeRanger: Triggas när SPELAREN kliver i den
        if (type == TriggerType.ResumeNPC && other.CompareTag("Player"))
        {
            Sgit.Instance.ResumeRunning();
        }
    }
}
