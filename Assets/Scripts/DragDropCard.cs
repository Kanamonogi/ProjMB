using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDropCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ─────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────
    [Header("Card Settings")]
    public float spawnCost = 10f;
    public GameObject monsterPrefab;

    [Header("Highlight (Optional)")]
    public SpriteRenderer spawnAreaRenderer;        // ลาก RedZone_Indicator มาใส่ได้ (จะเปลี่ยนสีตอนลาก)
    public Color highlightColor = new Color(0f, 1f, 0f, 0.4f);
    private Color originalColor;

    // ─────────────────────────────────────────
    // PRIVATE
    // ─────────────────────────────────────────
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Camera mainCamera;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        mainCamera = Camera.main;

        originalPosition = rectTransform.anchoredPosition;

        if (spawnAreaRenderer != null)
            originalColor = spawnAreaRenderer.color;
    }

    // ─────────────────────────────────────────
    // 1. เริ่มลาก
    // ─────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
        }

        if (spawnAreaRenderer != null)
            spawnAreaRenderer.color = highlightColor;
    }

    // ─────────────────────────────────────────
    // 2. ระหว่างลาก — ล็อกติดปลายเมาส์เป๊ะๆ
    // ─────────────────────────────────────────
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // ─────────────────────────────────────────
    // 3. ปล่อย — เช็ค Frontline และหักแต้มวิญญาณ
    // ─────────────────────────────────────────
    public void OnEndDrag(PointerEventData eventData)
    {
        // รีเซ็ต UI
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        if (spawnAreaRenderer != null)
            spawnAreaRenderer.color = originalColor;

        // แปลงตำแหน่งที่ปล่อย → World Space
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
            new Vector3(eventData.position.x, eventData.position.y, 0f)
        );
        worldPos.z = 0f;
        
        // ล็อกแกน Y ให้มอนสเตอร์ตกพื้นระดับเดียวกันเสมอ (ปรับแก้ตามระดับพื้นแมพคุณได้)
        worldPos.y = -2f; 

        // เช็คเขต Frontline ว่าอยู่ในพื้นที่อนุญาตไหม
        bool isValidZone = FrontlineManager.Instance.IsValidSpawnPosition(worldPos.x);

        // เช็คแต้มวิญญาณผ่าน GameManager ตัวใหม่ (หักแต้มทันทีหากพอ)
        bool hasEnoughPoints = GameManager.Instance.SpendSoulPoints(spawnCost);

        if (isValidZone && hasEnoughPoints)
        {
            // ✅ ผ่านเงื่อนไขทั้งคู่ -> เสกมอนสเตอร์
            SpawnMonster(worldPos);
        }
        else if (!isValidZone && hasEnoughPoints)
        {
            // ❌ วางผิดที่ แต่ดันตัดแต้มไปแล้ว -> คืนแต้มให้ผู้เล่น
            GameManager.Instance.AddSoulPoints(spawnCost);
            Debug.Log("[DragDropCard] Dropped in Red Zone! Soul Points refunded.");
        }

        // ส่งการ์ดกลับเข้า Deck แผงด้านล่างเสมอ
        SnapBack();
    }

    // ─────────────────────────────────────────
    // เสกมอนสเตอร์ + Register กับ FrontlineManager
    // ─────────────────────────────────────────
    void SpawnMonster(Vector3 position)
    {
        if (monsterPrefab == null)
        {
            Debug.LogWarning("[DragDropCard] Monster Prefab is not assigned!");
            return;
        }

        GameObject newMonster = Instantiate(monsterPrefab, position, Quaternion.identity);

        // แจ้ง FrontlineManager ให้จับตาดูความสูงแกน X ของมอนสเตอร์ตัวนี้
        if (FrontlineManager.Instance != null)
        {
            FrontlineManager.Instance.RegisterMonster(newMonster.transform);
        }

        Debug.Log($"[DragDropCard] Monster spawned at {position}");
    }

    void SnapBack()
    {
        rectTransform.anchoredPosition = originalPosition;
    }
}