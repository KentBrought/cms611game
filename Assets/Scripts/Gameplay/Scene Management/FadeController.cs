using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [SerializeField]
    private Image m_FadeImage;
    
    [SerializeField]
    private CanvasGroup m_CanvasGroup;
    
    private void Awake()
    {
        // Create fade UI if not assigned
        if (m_FadeImage == null)
        {
            CreateFadeUI();
        }
    }
    
    private void CreateFadeUI()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // High sorting order to appear on top
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Create Image
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        
        m_FadeImage = imageGO.AddComponent<Image>();
        m_FadeImage.color = Color.black;
        
        RectTransform rectTransform = m_FadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        m_CanvasGroup = imageGO.AddComponent<CanvasGroup>();
        m_CanvasGroup.alpha = 0f;
        
        DontDestroyOnLoad(canvasGO);
    }
    
    public IEnumerator FadeOut(float duration)
    {
        if (m_CanvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            m_CanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }
        m_CanvasGroup.alpha = 1f;
    }
    
    public IEnumerator FadeIn(float duration)
    {
        if (m_CanvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            m_CanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            yield return null;
        }
        m_CanvasGroup.alpha = 0f;
    }
}
