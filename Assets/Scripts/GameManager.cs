using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ─────────────────────────────────────────
    // BASE HP
    // ─────────────────────────────────────────
    [Header("Base HP")]
    public float maxBaseHP = 100f;
    private float currentBaseHP;

    // ─────────────────────────────────────────
    // SOUL POINTS
    // ─────────────────────────────────────────
    [Header("Soul Points")]
    public float maxSoulPoints = 100f;
    public float startingSoulPoints = 20f;
    public float soulPointsPerSecond = 5f;
    private float currentSoulPoints;

    private float soulRegenTimer = 0f;

    // ─────────────────────────────────────────
    // EVENTS (UI will listen to these)
    // ─────────────────────────────────────────
    public UnityEvent<float, float> onBaseHPChanged;     // (current, max)
    public UnityEvent<float, float> onSoulPointsChanged; // (current, max)
    public UnityEvent onGameOver;

    // ─────────────────────────────────────────
    // SINGLETON SETUP
    // ─────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Initialize values
        currentBaseHP = maxBaseHP;
        currentSoulPoints = startingSoulPoints;

        // Notify UI of starting values
        onBaseHPChanged?.Invoke(currentBaseHP, maxBaseHP);
        onSoulPointsChanged?.Invoke(currentSoulPoints, maxSoulPoints);
    }

    // ─────────────────────────────────────────
    // SOUL POINTS REGEN (every second)
    // ─────────────────────────────────────────
    void Update()
    {
        soulRegenTimer += Time.deltaTime;

        if (soulRegenTimer >= 1f)
        {
            soulRegenTimer = 0f;
            AddSoulPoints(soulPointsPerSecond);
        }
    }

    // ─────────────────────────────────────────
    // SOUL POINTS: PUBLIC FUNCTIONS
    // ─────────────────────────────────────────

    // เพิ่มแต้มวิญญาณ (ใช้ใน Regen + อื่นๆ)
    public void AddSoulPoints(float amount)
    {
        currentSoulPoints = Mathf.Min(currentSoulPoints + amount, maxSoulPoints);
        onSoulPointsChanged?.Invoke(currentSoulPoints, maxSoulPoints);
    }

    // หักแต้มวิญญาณ — คืนค่า true ถ้าหักสำเร็จ, false ถ้าแต้มไม่พอ
    // Step 2 (Drag & Drop) จะเรียกใช้ฟังก์ชันนี้
    public bool SpendSoulPoints(float amount)
    {
        if (currentSoulPoints < amount)
        {
            Debug.Log($"[GameManager] Not enough Soul Points! Have: {currentSoulPoints}, Need: {amount}");
            return false;
        }

        currentSoulPoints -= amount;
        onSoulPointsChanged?.Invoke(currentSoulPoints, maxSoulPoints);
        return true;
    }

    // เช็คแต้มวิญญาณปัจจุบัน (ไว้ให้ UI หรือระบบอื่นดึงค่า)
    public float GetSoulPoints() => currentSoulPoints;

    // ─────────────────────────────────────────
    // BASE HP: PUBLIC FUNCTIONS
    // ─────────────────────────────────────────

    // ฐานโดนโจมตี — Step 3 (Combat AI) จะเรียกใช้
    public void DamageBase(float amount)
    {
        currentBaseHP = Mathf.Max(currentBaseHP - amount, 0f);
        onBaseHPChanged?.Invoke(currentBaseHP, maxBaseHP);
        Debug.Log($"[GameManager] Base took {amount} damage! HP: {currentBaseHP}/{maxBaseHP}");

        if (currentBaseHP <= 0f)
        {
            TriggerGameOver();
        }
    }

    // ฮีลฐาน (ถ้าต้องการในอนาคต)
    public void HealBase(float amount)
    {
        currentBaseHP = Mathf.Min(currentBaseHP + amount, maxBaseHP);
        onBaseHPChanged?.Invoke(currentBaseHP, maxBaseHP);
    }

    public float GetBaseHP() => currentBaseHP;

    // ─────────────────────────────────────────
    // GAME OVER
    // ─────────────────────────────────────────
    private void TriggerGameOver()
    {
        Debug.Log("[GameManager] *** GAME OVER *** Base HP reached 0!");
        onGameOver?.Invoke();

        // TODO: หยุดเกม / โชว์หน้าจอ Game Over ใน Step ถัดๆ ไป
        Time.timeScale = 0f;
    }
}