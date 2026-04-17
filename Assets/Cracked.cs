using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cracked : MonoBehaviour
{
   
    public float explosionForce = 1000f; // The force applied to objects in the explosion.
    public float explosionRadius = 5f; // The radius of the explosion.

    public float startFadeAfter = 2f; // Vänta 2 sek innan fade börjar
    public float fadeSpeed = 0.5f;

    private void Start()
    {
        Explode();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            // Detta skapar en unik instans av materialet för just denna del
            // r.material = new Material(r.material);
            // För URP (Universal Render Pipeline)
            r.material.SetFloat("_Surface", 1); // 1 = Transparent, 0 = Opaque
            r.material.SetOverrideTag("RenderType", "Transparent");
            r.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            r.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            r.material.SetInt("_ZWrite", 0);
            r.material.DisableKeyword("_ALPHATEST_ON");
            r.material.EnableKeyword("_ALPHABLEND_ON");
            r.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        StartCoroutine(FadeOut());
    }


    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(startFadeAfter);

        float alpha = 1f;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            foreach (Renderer r in renderers)
            {
                Color c = r.material.color; // Eller "_BaseColor" om du använder URP
                c.a = alpha;
                r.material.color = c;
            }
            yield return null; // Viktigt! Vänta till nästa bildruta
        }
        Destroy(gameObject, 2);
    }


    void Explode()
    {
       

       

        // Get all colliders within the explosion radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Apply explosion force to nearby objects with rigidbodies.
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

       
    }
}
