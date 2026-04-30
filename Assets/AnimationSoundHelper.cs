using UnityEngine;

public class AnimationSoundHelper : MonoBehaviour
{
    public void PlayBonusSound()
    {
        // Hitta skriptet där ljudet faktiskt finns och kör det
        GameObject.FindObjectOfType<Bolts>().Playsound();
    }
}
