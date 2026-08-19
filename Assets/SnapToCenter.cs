using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SnapToCenter : MonoBehaviour, IEndDragHandler
{
    [Header("References")]
    public ScrollRect scrollRect;
    public RectTransform viewport;
    public RectTransform content;

    [Header("Settings")]
    public float snapSpeed = 12f;

    private bool isSnapping = false;
    private Vector2 targetPosition;

    private void Start()
    {
        if (scrollRect == null) scrollRect = GetComponent<ScrollRect>();
        if (viewport == null && scrollRect != null) viewport = scrollRect.viewport;
        if (content == null && scrollRect != null) content = scrollRect.content;
    }

    private void Update()
    {
        if (isSnapping)
        {
            // Mjuk rörelse mot mitten av det valda objektet
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, Time.deltaTime * snapSpeed);

            // Stoppa när vi är tillräckligt nära
            if (Vector2.Distance(content.anchoredPosition, targetPosition) < 0.1f)
            {
                content.anchoredPosition = targetPosition;
                isSnapping = false;
            }
        }
    }

    // Körs automatiskt när spelaren släpper musen/scrollen
    public void OnEndDrag(PointerEventData eventData)
    {
        SnapToNearest();
    }

    public void SnapToNearest()
    {
        if (content.childCount == 0) return;

        float minDistance = float.MaxValue;
        RectTransform targetChild = null;

        Vector3 viewportCenter = viewport.position;

        // Hitta det objekt som är närmast mitten av Viewport
        foreach (RectTransform child in content)
        {
            if (!child.gameObject.activeSelf) continue;

            float distance = Vector3.Distance(child.position, viewportCenter);
            if (distance < minDistance)
            {
                minDistance = distance;
                targetChild = child;
            }
        }

        if (targetChild != null)
        {
            // Beräkna Y-positionen så att barnet hamnar i mitten
            float targetY = -targetChild.anchoredPosition.y - (viewport.rect.height / 2f) + (targetChild.rect.height / 2f);
            targetPosition = new Vector2(content.anchoredPosition.x, targetY);
            
            scrollRect.velocity = Vector2.zero; // Stoppa tröghets-farten
            isSnapping = true;
        }
    }
}