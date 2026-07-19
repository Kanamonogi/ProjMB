using UnityEngine;
using System.Collections.Generic;

public class FrontlineManager : MonoBehaviour
{
    public static FrontlineManager Instance { get; private set; }

    // ─────────────────────────────────────────
    // INSPECTOR
    // ─────────────────────────────────────────
    [Header("Frontline Settings")]
    public float defaultMaxSpawnX = 0f;     // ตำแหน่งเริ่มต้น (กลางแผนที่)
    public float mapMinX = -20f;            // ขอบซ้ายสุดของแผนที่

    [Header("Red Zone Indicator")]
    public Transform redZoneIndicator;      // ลาก RedZone_Indicator Sprite มาใส่

    // ─────────────────────────────────────────
    // PRIVATE
    // ─────────────────────────────────────────
    private float maxSpawnX;
    private List<Transform> playerMonsters = new List<Transform>();

    // ─────────────────────────────────────────
    // SINGLETON
    // ─────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        maxSpawnX = defaultMaxSpawnX;
        UpdateRedZone();
    }

    // ─────────────────────────────────────────
    // UPDATE — หา Monster ที่อยู่ไกลสุดทุก Frame
    // ─────────────────────────────────────────
    void Update()
    {
        if (playerMonsters.Count == 0) return;

        // ลบ Monster ที่ถูก Destroy ออกก่อน
        playerMonsters.RemoveAll(m => m == null);

        // หา X ที่ไกลที่สุด
        float furthestX = defaultMaxSpawnX;
        foreach (Transform monster in playerMonsters)
        {
            if (monster.position.x > furthestX)
                furthestX = monster.position.x;
        }

        // Frontline ขยายได้อย่างเดียว ไม่ถอยหลัง
        if (furthestX > maxSpawnX)
        {
            maxSpawnX = furthestX;
            UpdateRedZone();
        }
    }

    // ─────────────────────────────────────────
    // อัปเดต RedZone Sprite ให้ขอบซ้ายตรงกับ maxSpawnX
    // ─────────────────────────────────────────
    void UpdateRedZone()
    {
        if (redZoneIndicator == null) return;

        // คำนวณความกว้างของ RedZone (จาก maxSpawnX ถึงขอบขวาแผนที่)
        float redZoneWidth = redZoneIndicator.GetComponent<SpriteRenderer>().bounds.size.x;

        // ขยับ Sprite ให้ขอบซ้ายอยู่ที่ maxSpawnX เสมอ
        // (pivot ของ Sprite ต้องตั้งเป็น Left ใน Sprite Editor หรือใช้ offset ด้านล่าง)
        float newCenterX = maxSpawnX + (redZoneWidth / 2f);
        redZoneIndicator.position = new Vector3(
            newCenterX,
            redZoneIndicator.position.y,
            redZoneIndicator.position.z
        );
    }

    // ─────────────────────────────────────────
    // PUBLIC FUNCTIONS
    // ─────────────────────────────────────────

    // DragDropCard เรียกตอนเสก Monster สำเร็จ
    public void RegisterMonster(Transform monsterTransform)
    {
        if (!playerMonsters.Contains(monsterTransform))
            playerMonsters.Add(monsterTransform);
    }

    // ตรวจสอบว่าพิกัดที่วางอยู่ใน Spawn Zone ไหม
    public bool IsValidSpawnPosition(float worldX)
    {
        return worldX <= maxSpawnX;
    }

    public float GetMaxSpawnX() => maxSpawnX;
}