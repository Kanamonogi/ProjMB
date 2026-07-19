using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("Data Asset")]
    public MonsterDataSO monsterData; // ช่องสำหรับลากแฟ้มประวัติมาใส่

    [Header("Identity")]
    public bool isPlayer = true;
    public int currentLevel = 1; // เผื่อไว้ทำระบบอัปเลเวลในอนาคต

    // ─────────────────────────────────────────
    // RUNTIME STATS (ซ่อนไว้เพราะเดี๋ยวไปดึงจาก SO มาทับอยู่ดี)
    // ─────────────────────────────────────────
    [HideInInspector] public float maxHP;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float attackDamage;
    [HideInInspector] public float attackRange;
    [HideInInspector] public float detectionRange;
    [HideInInspector] public float attackCooldown;
    [HideInInspector] public float specialDamage;
    [HideInInspector] public float specialCooldown;

    // ─────────────────────────────────────────
    // ตัวแปรส่วนตัวที่ใช้จัดการระบบภายใน (ซ่อนไว้อยู่แล้ว)
    // ─────────────────────────────────────────
    private float currentHP;
    private float attackTimer = 0f;
    private float specialTimer = 0f;
    private bool isDead = false;          // ✅ เพิ่มกลับมา: เช็กสถานะการตาย
    private Transform currentTarget;      // ✅ เพิ่มกลับมา: เก็บเป้าหมายเป้าหมายที่กำลังตี
    private string enemyMonsterTag;       // ✅ เพิ่มกลับมา: เก็บชื่อ Tag ของศัตรู
    private string enemyBaseTag;          // ✅ เพิ่มกลับมา: เก็บชื่อ Tag ของป้อมศัตรู
    private Rigidbody2D rb;
    private Animator anim;
    

    void Start()
    {
        anim = GetComponent<Animator>();

        if (monsterData != null)
        {
            maxHP = monsterData.maxHP;
            moveSpeed = monsterData.moveSpeed;
            attackDamage = monsterData.attackDamage;
            attackRange = monsterData.attackRange;
            detectionRange = monsterData.detectionRange;
            attackCooldown = monsterData.attackCooldown;
            specialDamage = monsterData.specialDamage;
            specialCooldown = monsterData.specialCooldown;
        }
        
        else
        {
            Debug.LogWarning($"[MonsterAI] {gameObject.name} ลืมใส่ MonsterDataSO! จะใช้ค่า Default ในโค้ดแทน");
        }

        currentHP = maxHP;
        specialTimer = 0f;

        enemyMonsterTag = isPlayer ? "EnemyMonster" : "PlayerMonster";
        enemyBaseTag    = isPlayer ? "EnemyBase"    : "PlayerBase";

        if (!isPlayer) 
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void Update()
    {
        if (isDead) return;
        
        // 🔵 ชาร์จหลอดฟ้า (Sub Skill) ตลอดเวลา
        if (specialTimer < specialCooldown)
        {
            specialTimer += Time.deltaTime;
        }

        Transform attackTarget = FindTargetInRadius(attackRange);

        if (attackTarget != null)
        {
            if (anim != null) anim.SetBool("isWalking", false); 
            
            currentTarget = attackTarget;
            HandleAttack();
            return;
        }

        Transform detected = FindTargetInRadius(detectionRange);

        if (detected != null)
        {
            if (anim != null) anim.SetBool("isWalking", true);
            MoveToward(detected.position);
        }
        else
        {
            if (anim != null) anim.SetBool("isWalking", true);
            MoveForward();
        }

        attackTimer = 0f; 
    }

    void MoveForward()
    {
        Vector3 direction = isPlayer ? Vector3.right : Vector3.left;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    void MoveToward(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition.x > transform.position.x) ? Vector3.right : Vector3.left;
        transform.localScale = new Vector3(direction == Vector3.right ? 1 : -1, 1, 1);
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

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

    void HandleAttack()
    {
        attackTimer += Time.deltaTime;
       if (specialTimer >= specialCooldown)
        {
            specialTimer = 0f;
            attackTimer = 0f;
            
            PerformSpecialAttack(currentTarget);
            return;
        }
    
        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            PerformAttack(currentTarget);
        }
        
    }

    void PerformAttack(Transform target)
    {
        if (target == null) return;

        if (anim != null) 
        {
            anim.SetTrigger("Attack"); 
        }
        else 
        {
            ExecuteDamage();
        }
    }

    public void ExecuteDamage()
    {
        if (currentTarget == null) return;

        MonsterAI targetMonster = currentTarget.GetComponent<MonsterAI>();
        if (targetMonster != null)
        {
            targetMonster.TakeDamage(attackDamage);
            Debug.Log($"[MonsterAI] {gameObject.name} ป๊าบเข้าให้! โดน {currentTarget.name} ดาเมจ {attackDamage}");
            return;
        }

        GameManager.Instance.DamageBase(attackDamage, isPlayer);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log($"[MonsterAI] {gameObject.name} took {damage} dmg | HP: {currentHP}/{maxHP}");

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        if (anim != null) anim.SetTrigger("Die");

        Debug.Log($"[MonsterAI] {gameObject.name} died.");
        Destroy(gameObject, 2f); 
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void PerformSpecialAttack(Transform target)
    {
        if (target == null) return;

        // ถ้าอนาคตคุณทำท่าสกิลใน Maya เสร็จ ค่อยมาเปลี่ยนชื่อ Trigger ตรงนี้เป็น "SpecialSkill" ได้ครับ
        // แต่ตอนนี้เราใช้ท่า "Attack" เดิมไปก่อนเพื่อทดสอบระบบดาเมจ
        if (anim != null)
        {
        anim.SetTrigger("Skill");     
        }
        else
        {
            ExecuteSpecialDamage();
        }
    }

    public void ExecuteSpecialDamage()
    {
        if (currentTarget == null) return;

        MonsterAI targetMonster = currentTarget.GetComponent<MonsterAI>();
        if (targetMonster != null)
        {
            targetMonster.TakeDamage(specialDamage);
            Debug.Log($"[MonsterAI-SKILL] 🔵 {gameObject.name} ใช้สกิลรอง! อัด {currentTarget.name} ดาเมจ {specialDamage}");
            return;
        }

        GameManager.Instance.DamageBase(specialDamage, isPlayer);
    }
}