using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Sgit : MonoBehaviour
{
    [Header("Komponenter")]
    public SplineAnimate splineAnimate;
    public Animator animator;

    [Header("Inställningar för nästa Spline (Om du vill använda flera)")]
    public SplineContainer nextSpline; // Fylls bara i om du kör med FLERA splines
    
    public static Sgit Instance;
    private bool isStopped = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Starta löpningen när scenen börjar
        StartRunning();
    }

    // --- 1. STOPPA OCH PAUSA ---
    public void StopAtLocation()
    {
        if (isStopped) return;

        isStopped = true;

        if (splineAnimate != null)
        {
            splineAnimate.Pause();
        }

        if (animator != null)
        {
            animator.SetBool("Run", false);
            //animator.SetTrigger("Wait"); 
        }
    }

    // --- 2. FORTSÄTT SPRINGA ---
    public void ResumeRunning()
    {
        if (!isStopped) return;

        isStopped = false;

        if (animator != null)
        {
            animator.SetBool("Run", true);
        }

        if (splineAnimate != null)
        {
            splineAnimate.Play();
        }
    }

    // --- 3. HOPPA (NY!) ---
    public void Jump()
    {
        if (animator != null)
        {
            animator.SetBool("Run", false);
            // Triggat hopp-animationen i Animator
            animator.SetTrigger("Jump");
        }
    }

    // --- 4. STARTA LÖPNING ---
    public void StartRunning()
    {
        isStopped = false;
        if (splineAnimate != null) splineAnimate.Play();
        if (animator != null) animator.SetBool("Run", true);
    }
}