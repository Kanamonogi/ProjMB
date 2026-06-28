using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ─────────────────────────────────────────
    // SOUL POINTS
    // ─────────────────────────────────────────
    [Header("Soul Points")]
    public float currentSoulPoints = 20f;
    public float maxSoulPoints = 100f;
    public float soulRegenRate = 5f;       // ต่อวินาที

    [Header("Soul Points UI")]
    public TextMeshProUGUI soulPointsText; // ลาก TMP Text มาใส่

    private float soulRegenTimer = 0f;

    // ─────────────────────────────────────────
    // BASE HP
    // ─────────────────────────────────────────
    [Header("Base HP")]
    public float playerBaseHP = 100f;
    public float maxPlayerBaseHP = 100f;
    public float enemyBaseHP = 100f;
    public float maxEnemyBaseHP = 100f;

    [Header("Base HP UI")]
    public Slider playerBaseSlider;        // ลาก Slider ของฝั่งผู้เล่นมาใส่

    private bool isGameOver = false;

    // ─────────────────────────────────────────
    // SINGLETON
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
        UpdateSoulPointsUI();
        UpdatePlayerBaseUI();
    }

    // ─────────────────────────────────────────
    // SOUL POINTS REGEN (every second)
    // ─────────────────────────────────────────
    void Update()
    {
        if (isGameOver) return;

        soulRegenTimer += Time.deltaTime;
        if (soulRegenTimer >= 1f)
        {
            soulRegenTimer = 0f;
            AddSoulPoints(soulRegenRate);
        }
    }

    // ─────────────────────────────────────────
    // SOUL POINTS: PUBLIC FUNCTIONS
    // (Signature kept identical — DragDropCard depends on this)
    // ─────────────────────────────────────────

    public bool SpendSoulPoints(float amount)
    {
        if (currentSoulPoints < amount)
        {
            Debug.Log($"[GameManager] Not enough Soul Points! Have: {currentSoulPoints}, Need: {amount}");
            return false;
        }

        currentSoulPoints -= amount;
        UpdateSoulPointsUI();
        return true;
    }

    public void AddSoulPoints(float amount)
    {
        currentSoulPoints = Mathf.Min(currentSoulPoints + amount, maxSoulPoints);
        UpdateSoulPointsUI();
    }

    void UpdateSoulPointsUI()
    {
        if (soulPointsText != null)
            soulPointsText.text = $"{Mathf.FloorToInt(currentSoulPoints)} / {Mathf.FloorToInt(maxSoulPoints)}";
    }

    // ─────────────────────────────────────────
    // BASE HP: PUBLIC FUNCTIONS
    // ─────────────────────────────────────────

    // attackerIsPlayer = true  → Player monster ตี → ลด enemyBaseHP
    // attackerIsPlayer = false → Enemy monster ตี  → ลด playerBaseHP
    public void DamageBase(float damage, bool attackerIsPlayer)
    {
        if (isGameOver) return;

        if (attackerIsPlayer)
        {
            // ผู้เล่นตีฐานศัตรู
            enemyBaseHP = Mathf.Max(enemyBaseHP - damage, 0f);
            Debug.Log($"[GameManager] Enemy Base took {damage} dmg! HP: {enemyBaseHP}/{maxEnemyBaseHP}");

            if (enemyBaseHP <= 0f)
                TriggerGameOver(playerWon: true);
        }
        else
        {
            // ศัตรูตีฐานผู้เล่น
            playerBaseHP = Mathf.Max(playerBaseHP - damage, 0f);
            UpdatePlayerBaseUI();
            Debug.Log($"[GameManager] Player Base took {damage} dmg! HP: {playerBaseHP}/{maxPlayerBaseHP}");

            if (playerBaseHP <= 0f)
                TriggerGameOver(playerWon: false);
        }
    }

    void UpdatePlayerBaseUI()
    {
        if (playerBaseSlider != null)
        {
            playerBaseSlider.maxValue = maxPlayerBaseHP;
            playerBaseSlider.value = playerBaseHP;
        }
    }

    // ─────────────────────────────────────────
    // GAME OVER
    // ─────────────────────────────────────────
    void TriggerGameOver(bool playerWon)
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log(playerWon
            ? "[GameManager] *** VICTORY *** Enemy Base destroyed!"
            : "[GameManager] *** GAME OVER *** Player Base destroyed!");

        // TODO: โชว์ Game Over / Victory UI ในขั้นถัดไป
        Time.timeScale = 0f;
    }
}