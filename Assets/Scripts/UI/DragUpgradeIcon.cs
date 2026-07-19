using UnityEngine;
using UnityEngine.EventSystems;

public class DragUpgradeIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // ถ้าลืมใส่ CanvasGroup ใน Unity โค้ดจะแอดให้เองอัตโนมัติ
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false; // ทะลุไปเช็กว่าโดนมอนสเตอร์ไหม
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // อนาคตจะเขียนโค้ดเช็กว่าลากไปชนมอนสเตอร์หรือเปล่าตรงนี้
        // ตอนนี้ให้เด้งกลับไปที่เดิมก่อน
        rectTransform.anchoredPosition = originalPosition;
    }
}