using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    // ─────────────────────────────────────────
    // IDENTITY & STATS
    // ─────────────────────────────────────────
    [Header("Identity")]
    public bool isPlayer = true;

    [Header("Stats")]
    public float maxHP = 50f;
    public float moveSpeed = 2f;
    public float attackDamage = 10f;
    public float attackRange = 1.2f;        // radius for OverlapCircleAll attack check
    public float detectionRange = 8f;       // radius for spotting targets
    public float attackCooldown = 1.5f;

    // ─────────────────────────────────────────
    // PRIVATE
    // ─────────────────────────────────────────
    private float currentHP;
    private float attackTimer = 0f;
    private Transform currentTarget;
    private bool isDead = false;

    private string enemyMonsterTag;
    private string enemyBaseTag;

    void Start()
    {
        currentHP = maxHP;

        // กำหนด Tag ของฝั่งตรงข้ามอัตโนมัติจาก isPlayer
        enemyMonsterTag = isPlayer ? "EnemyMonster" : "PlayerMonster";
        enemyBaseTag    = isPlayer ? "EnemyBase"    : "PlayerBase";
    }

    void Update()
    {
        if (isDead) return;

        // ── STEP 1: เช็คว่ามีอะไรอยู่ในระยะ "attackRange" ไหม (edge-to-edge ผ่าน Collider) ──
        Transform attackTarget = FindTargetInRadius(attackRange);

        if (attackTarget != null)
        {
            // มีศัตรูอยู่ในระยะตี → หยุดเดิน, โจมตี
            currentTarget = attackTarget;
            HandleAttack();
            return;
        }

        // ── STEP 2: ไม่มีอะไรอยู่ในระยะตี → เช็คว่ามีศัตรูอยู่ใน detectionRange ไหม ──
        Transform detected = FindTargetInRadius(detectionRange);

        if (detected != null)
        {
            // เห็นศัตรูแต่ยังไกล → เดินเข้าหา
            MoveToward(detected.position);
        }
        else
        {
            // ไม่เห็นอะไรเลย → เดินหน้าตามทิศทางทีม
            MoveForward();
        }

        // รีเซ็ต timer ถ้าหลุดระยะตีไปแล้ว (ป้องกันการตีทันทีที่กลับเข้าระยะ)
        attackTimer = 0f;
    }

    // ─────────────────────────────────────────
    // MOVEMENT
    // ─────────────────────────────────────────
    void MoveForward()
    {
        Vector3 direction = isPlayer ? Vector3.right : Vector3.left;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    void MoveToward(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition.x > transform.position.x) ? Vector3.right : Vector3.left;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    // ─────────────────────────────────────────
    // DETECTION / RANGE CHECK
    // CRITICAL FIX: Uses Physics2D.OverlapCircleAll (edge-to-edge via colliders)
    // instead of Vector2.Distance (center-to-center), so attacks correctly
    // register against large BoxCollider2D bases.
    // ─────────────────────────────────────────
    Transform FindTargetInRadius(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == this.gameObject) continue;

            bool isValidTarget = hit.CompareTag(enemyMonsterTag) || hit.CompareTag(enemyBaseTag);
            if (!isValidTarget) continue;

            // ใช้ ClosestPoint เพื่อหาระยะจากขอบ Collider จริงๆ (รองรับ Base ที่เป็น BoxCollider2D ใหญ่)
            Vector2 closestPoint = hit.ClosestPoint(transform.position);
            float dist = Vector2.Distance(transform.position, closestPoint);

            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = hit.transform;
            }
        }

        return nearest;
    }

    // ─────────────────────────────────────────
    // COMBAT
    // ─────────────────────────────────────────
    void HandleAttack()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            PerformAttack(currentTarget);
        }
    }

    void PerformAttack(Transform target)
    {
        if (target == null) return;

        // ── ใส่ Attack Animation / VFX ตรงนี้ในอนาคต ──

        // ลองหา MonsterAI ก่อน (ถ้าเป้าหมายเป็น Monster)
        MonsterAI targetMonster = target.GetComponent<MonsterAI>();
        if (targetMonster != null)
        {
            targetMonster.TakeDamage(attackDamage);
            Debug.Log($"[MonsterAI] {gameObject.name} attacked monster {target.name} for {attackDamage} dmg.");
            return;
        }

        // ถ้าไม่ใช่ Monster → ต้องเป็น Base → แจ้ง GameManager พร้อมระบุฝั่งผู้โจมตี
        GameManager.Instance.DamageBase(attackDamage, isPlayer);
        Debug.Log($"[MonsterAI] {gameObject.name} attacked base {target.name} for {attackDamage} dmg.");
    }

    // ─────────────────────────────────────────
    // TAKE DAMAGE & DEATH
    // ─────────────────────────────────────────
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;

        // ── ใส่ Hit Animation / VFX ตรงนี้ในอนาคต ──

        Debug.Log($"[MonsterAI] {gameObject.name} took {damage} dmg | HP: {currentHP}/{maxHP}");

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // ── ใส่ Death Animation / VFX ตรงนี้ในอนาคต ──

        Debug.Log($"[MonsterAI] {gameObject.name} died.");
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────
    // DEBUG GIZMOS
    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}