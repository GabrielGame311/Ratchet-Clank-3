using UnityEngine;

public class ScrollFocusScaler : MonoBehaviour
{
    [Header("References")]
    public RectTransform viewport;
    public RectTransform content;

    [Header("Scale Settings")]
    public Vector3 selectedScale = new Vector3(1.1f, 1.1f, 1f);   // Storlek på det valda kortet i mitten
    public Vector3 unselectedScale = new Vector3(0.65f, 0.65f, 1f); // Storlek på övriga kort
    public float maxDistance = 250f;                              // Hur snabbt de ska krympa när man scrollar ifrån dem

    private void Update()
    {
        if (content == null || viewport == null) return;

        float distanceLimit = Mathf.Max(1f, maxDistance);
        Vector3 viewportCenter = viewport.rect.center;
        RectTransform focusedChild = null;
        float closestDistance = float.MaxValue;

        foreach (RectTransform child in content)
        {
            if (!child.gameObject.activeSelf) continue;

            // Compare both UI elements in the viewport's local coordinate space.
            Vector3 childCenter = viewport.InverseTransformPoint(child.TransformPoint(child.rect.center));
            float distance = Mathf.Abs(childCenter.y - viewportCenter.y);

            // Omvandla avståndet till ett faktor-värde mellan 0 (mitten) och 1 (långt bort)
            float t = Mathf.Clamp01(distance / distanceLimit);

            // Ändra storleken mjukt baserat på avståndet
            child.localScale = Vector3.Lerp(selectedScale, unselectedScale, t);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                focusedChild = child;
            }
        }

        foreach (RectTransform child in content)
        {
            LoadGame loadGame = child.GetComponent<LoadGame>();
            if (loadGame == null || loadGame.detailsPanel == null) continue;

            // Only the card nearest the viewport center shows its details.
            bool isFocused = child == focusedChild;
            if (loadGame.detailsPanel.activeSelf != isFocused)
            {
                loadGame.detailsPanel.SetActive(isFocused);
            }
        }
    }
}