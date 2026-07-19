using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "ScriptableObjects/MonsterData", order = 1)]
public class MonsterDataSO : ScriptableObject
{
    [Header("Display Settings")]
    public string monsterName;         // ชื่อมอนสเตอร์
    public Sprite cardIcon;            // รูปไอคอนการ์ดที่จะโชว์บนหน้าจอ

    [Header("Combat Stats")]
    public float maxHP = 50f;          // เลือดสูงสุด
    public float moveSpeed = 2f;       // ความเร็วในการเดิน
    public float attackDamage = 10f;   // พลังโจมตี
    public float attackRange = 1.2f;   // ระยะโจมตี
    public float detectionRange = 8f;  // ระยะมองเห็นศัตรู
    public float attackCooldown = 1.5f;// ความเร็วในการตี (วินาทีต่อครั้ง)

    [Header("Economy Settings")]
    public float spawnCost = 10f;      // แต้ม Soul ที่ใช้ในการเสกตัวนี้
}