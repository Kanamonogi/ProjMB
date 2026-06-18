using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    // ─────────────────────────────────────────
    // IDENTITY & STATS
    // ─────────────────────────────────────────
    [Header("Identity")]
    public bool isPlayer = true;            // true = ฝั่งผู้เล่น, false = ฝั่งศัตรู

    [Header("Stats")]
    public float maxHP = 50f;
    public float moveSpeed = 2f;
    public float attackDamage = 10f;
    public float attackRange = 1.2f;        // ระยะโจมตี
    public float detectionRange = 8f;       // ระยะมองเห็นศัตรู
    public float attackCooldown = 1.5f;     // วินาทีต่อการโจมตี 1 ครั้ง

    // ─────────────────────────────────────────
    // PRIVATE
    // ─────────────────────────────────────────
    private float currentHP;
    private float attackTimer = 0f;         // นับเวลาระหว่างการโจมตี
    private Transform currentTarget;        // เป้าหมายปัจจุบัน
    private bool isDead = false;

    // Tags ฝั่งตรงข้าม (เซ็ตอัตโนมัติจาก isPlayer)
    private string enemyMonsterTag;
    private string enemyBaseTag;

    // ─────────────────────────────────────────
    // STATE MACHINE
    // ─────────────────────────────────────────
    private enum State { Moving, Attacking }
    private State currentState = State.Moving;

    // ─────────────────────────────────────────
    // INIT
    // ─────────────────────────────────────────
    void Start()
    {
        currentHP = maxHP;

        // กำหนด Tag ศัตรูตาม isPlayer
        enemyMonsterTag = isPlayer ? "EnemyMonster" : "PlayerMonster";
        enemyBaseTag    = isPlayer ? "EnemyBase"    : "PlayerBase";
    }

    // ─────────────────────────────────────────
    // MAIN LOOP
    // ─────────────────────────────────────────
    void Update()
    {
        if (isDead) return;

        // หาเป้าหมายที่ใกล้ที่สุดในระยะ detectionRange
        currentTarget = FindNearestTarget();

        if (currentTarget != null)
        {
            float distToTarget = Vector2.Distance(transform.position, currentTarget.position);

            if (distToTarget <= attackRange)
            {
                // ศัตรูอยู่ในระยะโจมตี → หยุดและโจมตี
                currentState = State.Attacking;
                HandleAttack();
            }
            else
            {
                // เห็นศัตรูแต่ยังไกลเกินไป → เดินเข้าหา
                currentState = State.Moving;
                MoveToward(currentTarget.position);
            }
        }
        else
        {
            // ไม่มีเป้าหมาย → เดินหน้าต่อ
            currentState = State.Moving;
            MoveForward();
        }
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
        // เดินเฉพาะแกน X เท่านั้น (2D Horizontal)
        Vector3 direction = (targetPosition.x > transform.position.x) ? Vector3.right : Vector3.left;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    // ─────────────────────────────────────────
    // DETECTION — หา Target ที่ใกล้สุด
    // ─────────────────────────────────────────
    Transform FindNearestTarget()
    {
        Transform nearest = null;
        float nearestDist = detectionRange;

        // เช็คทั้ง Monster ฝั่งตรงข้าม และ Base ฝั่งตรงข้าม
        nearest = CheckTag(enemyMonsterTag, nearest, ref nearestDist);
        nearest = CheckTag(enemyBaseTag,    nearest, ref nearestDist);

        return nearest;
    }

    Transform CheckTag(string tag, Transform currentNearest, ref float nearestDist)
    {
        // ใช้ OverlapCircle หา Collider ทุกอันในระยะ detectionRange
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag(tag)) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                currentNearest = hit.transform;
            }
        }

        return currentNearest;
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

        // ── ใส่ Animation / VFX ตรงนี้ในอนาคต ──
        // OnAttackAnimationTrigger();

        // พยายาม Damage MonsterAI ก่อน
        MonsterAI targetMonster = target.GetComponent<MonsterAI>();
        if (targetMonster != null)
        {
            targetMonster.TakeDamage(attackDamage);
            return;
        }

        // ถ้าไม่ใช่ Monster → พยายาม Damage Base
        // Step 4 จะเพิ่ม BaseHP script → เรียก TakeDamage ที่นั่น
        GameManager.Instance.DamageBase(attackDamage);

        Debug.Log($"[MonsterAI] {gameObject.name} attacked {target.name} for {attackDamage} damage.");
    }

    // ─────────────────────────────────────────
    // TAKE DAMAGE & DEATH
    // ─────────────────────────────────────────
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;

        // ── ใส่ Hit Animation / VFX ตรงนี้ในอนาคต ──
        // OnHitAnimationTrigger();

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
        // OnDeathAnimationTrigger();

        Debug.Log($"[MonsterAI] {gameObject.name} died.");
        Destroy(gameObject);
    }

    // ─────────────────────────────────────────
    // DEBUG — แสดง Detection Range ใน Scene View
    // ─────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // วงสีเหลือง = Detection Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // วงสีแดง = Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}