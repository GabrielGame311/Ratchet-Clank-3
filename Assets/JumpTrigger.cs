using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class JumpTrigger : MonoBehaviour
{
    [Header("Inställningar")]
    [Tooltip("Hur många sekunder gubben är i luften innan alignment återställs")]
    public float jumpAirTime = 1.0f;

    private void OnTriggerEnter(Collider other)
    {
        // Kolla om det är NPC:n som kliver i triggern
        if (other.CompareTag("NPC") || other.GetComponentInParent<Sgit>() != null)
        {
            Sgit sgit = other.GetComponentInParent<Sgit>();
            SplineAnimate splineAnimate = other.GetComponentInParent<SplineAnimate>();

            if (sgit != null)
            {
                // 1. Trigga hoppet
                sgit.Jump();

                // 2. Ändra alignment under luftsprånget
                if (splineAnimate != null)
                {
                    StartCoroutine(TemporarilyDisableAlignment(splineAnimate));
                }
            }
        }
    }

    private IEnumerator TemporarilyDisableAlignment(SplineAnimate splineAnimate)
    {
        // RÄTT ENUM: SplineAnimate.AlignmentMode
        SplineAnimate.AlignmentMode originalAlignment = splineAnimate.Alignment;

        // Stäng av rotationen så att gubben inte vrider sig konstigt i luften
        splineAnimate.Alignment = SplineAnimate.AlignmentMode.None;

        // Vänta under tiden gubben är i luften
        yield return new WaitForSeconds(jumpAirTime);

        // Återställ ursprunglig alignment när han landat
        splineAnimate.Alignment = originalAlignment;
    }
}