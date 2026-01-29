using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonSpawnGame : MonoBehaviour
{
    public RectTransform targetZone;
    public Transform spawnUppParent;
    public float moveSpeed;
    public string KeyText;

    public static ButtonSpawnGame currentActiveKey;

    public bool isHandled = false; // ← så de inte räknas som klara direkt

    public bool isInZone = false;
    private Animator anime;
    private RectTransform rectTransform;
    public float Venster = -200;
    
    void Start()
    {
       
       
        rectTransform = GetComponent<RectTransform>();
        anime = GetComponent<Animator>();
        GetComponentInChildren<TMP_Text>().text = KeyText;
        isHandled = false;
        if (currentActiveKey == null)
            currentActiveKey = this;
       
    }

    void Update()
    {

       

        isInZone = IsInZone();

        

        if (GameThyrra_UI.IsOn)
        {
            rectTransform.anchoredPosition += Vector2.left * moveSpeed * Time.deltaTime;
        }

        if (rectTransform.anchoredPosition.x <= -350)
        {
            if(!isHandled)
            {
                HandleOutOfBounds();
            }
            return;
        }

        if (rectTransform.anchoredPosition.x <= Venster)
        {
            if (currentActiveKey == this)
                currentActiveKey = null;

            anime.SetTrigger("Hide");
            StartCoroutine(DestroyAfterDelay());
            return;
        }

        if (!GameThyrra_UI.isThyrraGame || !GameThyrra_UI.IsOn || isHandled)
            return;

        if (GameThyrra_UI.Instance.currentActiveKey != this)
            return;
       
        // Döljs om utanför vänsterkanten


        // Endast aktiv knapp reagerar


    }


    private void HandleOutOfBounds()
    {
        if (currentActiveKey == this)
            currentActiveKey = null;
        GetComponent<Image>().color = Color.red;
        GameThyrra_UI.isThyrraGame = false;
        GameThyrra_UI.IsOn = false;
       
    }


    public bool IsInZone()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(targetZone,rectTransform.position);




    }















    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        GameThyrra_UI.Instance.Buttons_.Remove(gameObject);
        Destroy(gameObject);
    }
}
