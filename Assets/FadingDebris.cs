using System.Collections;
using UnityEngine;

public class FadingDebris : MonoBehaviour
{
    public float flyForce = 14f;
    public float upwardForce = 4f;
    public float lifeTime = 1.0f;       // Tid innan den börjar tona ut
    public float fadeDuration = 0.8f;   // Tid det tar att tona ut helt

    private MeshRenderer meshRenderer;
    private Color originalColor;

    public void Launch(Vector3 direction)
    {
        transform.SetParent(null); 

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.AddForce(direction * flyForce + Vector3.up * upwardForce, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * flyForce * 3f, ForceMode.Impulse);

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.material.HasProperty("_Color"))
        {
            originalColor = meshRenderer.material.color;
        }

        StartCoroutine(FadeAndDestroyRoutine());
    }

    private IEnumerator FadeAndDestroyRoutine()
    {
        yield return new WaitForSeconds(lifeTime);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            if (meshRenderer != null && meshRenderer.material.HasProperty("_Color"))
            {
                Color c = originalColor;
                c.a = alpha;
                meshRenderer.material.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}