using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("Data Asset")]
    public MonsterDataSO monsterData; // ช่องสำหรับลากแฟ้มประวัติมาใส่

    [Header("Identity")]
    public bool isPlayer = true;

    [Header("Stats")]
    public float maxHP = 50f;
    public float moveSpeed = 2f;
    public float attackDamage = 10f;
    public float attackRange = 1.2f;        
    public float detectionRange = 8f;       
    public float attackCooldown = 1.5f;

    private float currentHP;
    private float attackTimer = 0f;
    private Transform currentTarget;
    private bool isDead = false;

    private string enemyMonsterTag;
    private string enemyBaseTag;
    
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
        }
        else
        {
            Debug.LogWarning($"[MonsterAI] {gameObject.name} ลืมใส่ MonsterDataSO! จะใช้ค่า Default ในโค้ดแทน");
        }

        currentHP = maxHP;

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

        Transform attackTarget = FindTargetInRadius(attackRange);

        if (attackTarget != null)
        {
            // ✅ [อัปเดต] เช็กก่อนว่ามี Animator ไหม ถ้ามีค่อยสั่งหยุดเดิน
            if (anim != null) anim.SetBool("isWalking", false); 
            
            currentTarget = attackTarget;
            HandleAttack();
            return;
        }

        Transform detected = FindTargetInRadius(detectionRange);

        if (detected != null)
        {
            // ✅ [อัปเดต] เช็กก่อนว่ามี Animator ไหม ถ้ามีค่อยสั่งเดิน
            if (anim != null) anim.SetBool("isWalking", true);
            MoveToward(detected.position);
        }
        else
        {
            // ✅ [อัปเดต] เช็กก่อนว่ามี Animator ไหม ถ้ามีค่อยสั่งเดิน
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

        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            PerformAttack(currentTarget);
        }
    }

    void PerformAttack(Transform target)
    {
        if (target == null) return;

        // ✅ [อัปเดต] เช็กก่อนว่ามี Animator ไหม ถ้ามีค่อยสั่งเล่นท่าง้างมือตี
        if (anim != null) 
        {
            anim.SetTrigger("Attack"); 
        }
        else 
        {
            // ถ้าเป็นแค่ Mockup (ไม่มีแอนิเมชัน) ให้มันหักเลือดศัตรูทันทีเลย
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

        // ✅ [อัปเดต] เช็กก่อนว่ามี Animator ไหม ถ้ามีค่อยสั่งเล่นท่าตาย
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
}