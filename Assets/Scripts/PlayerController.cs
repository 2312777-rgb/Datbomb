using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    public int maxBombs = 1;
    public int explosionRange = 1;

    [Header("Shield Settings")]
    public bool hasShield = false;

    [Tooltip("Hiệu ứng khiên quanh player")]
    public GameObject shieldVisual;

    [Tooltip("Icon khiên trên UI")]
    public GameObject shieldUIIcon;

    [Tooltip("Thời gian tồn tại khiên")]
    public float shieldDuration = 5f;

    [Header("UI Display")]
    public TextMeshProUGUI maxBombsText;
    public TextMeshProUGUI explosionRangeText;

    private Rigidbody2D rb;
    private Animator animator;
    private Grid grid;

    private Vector2 movement;

    private float lastHorizontal = 0f;
    private float lastVertical = -1f;

    private Coroutine shieldCoroutine;

    // Chống mất máu nhiều lần
    private bool isInvincible = false;

    private List<GameObject> activeBombsList = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        grid = FindAnyObjectByType<Grid>();

        // Rigidbody setup
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Đưa player vào giữa ô
        SnapToGrid();

        // Ẩn khiên lúc đầu
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        // Ẩn icon khiên lúc đầu
        if (shieldUIIcon != null)
        {
            shieldUIIcon.SetActive(false);
        }

        UpdateItemUI();
    }

    void Update()
    {
        // Xóa bomb null
        activeBombsList.RemoveAll(bomb => bomb == null);

        // Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Không đi chéo
        if (movement.x != 0)
        {
            movement.y = 0;
        }

        // Lưu hướng cuối
        if (movement.sqrMagnitude > 0)
        {
            lastHorizontal = movement.x;
            lastVertical = movement.y;
        }

        // Animation
        if (animator != null)
        {
            animator.SetFloat("Horizontal", movement.sqrMagnitude > 0 ? movement.x : lastHorizontal);
            animator.SetFloat("Vertical", movement.sqrMagnitude > 0 ? movement.y : lastVertical);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }

        // Đặt bomb
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlaceBomb();
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // =========================
    // PLACE BOMB
    // =========================

    void PlaceBomb()
    {
        if (bombPrefab == null || grid == null) return;

        // Giới hạn số bomb
        if (activeBombsList.Count >= maxBombs) return;

        // Lấy ô grid
        Vector3Int cellPos = grid.WorldToCell(transform.position);

        // Lấy giữa ô
        Vector3 bombPos = grid.GetCellCenterWorld(cellPos);

        // Không cho đặt chồng bomb
        foreach (GameObject bomb in activeBombsList)
        {
            if (bomb != null && Vector3.Distance(bomb.transform.position, bombPos) < 0.1f)
            {
                return;
            }
        }

        // Tạo bomb
        GameObject newBomb = Instantiate(bombPrefab, bombPos, Quaternion.identity);

        // Truyền range cho bomb
        Bomb bombScript = newBomb.GetComponent<Bomb>();

        if (bombScript != null)
        {
            bombScript.explosionRange = explosionRange;
        }

        activeBombsList.Add(newBomb);
    }

    // =========================
    // UI
    // =========================

    public void UpdateItemUI()
    {
        if (maxBombsText != null)
        {
            maxBombsText.text = "x" + maxBombs;
        }

        if (explosionRangeText != null)
        {
            explosionRangeText.text = "x" + explosionRange;
        }
    }

    public void IncreaseExplosionRange(int amount)
    {
        explosionRange += amount;
        UpdateItemUI();
    }

    public void IncreaseMaxBombs(int amount)
    {
        maxBombs += amount;
        UpdateItemUI();
    }

    // =========================
    // SHIELD
    // =========================

    public void ActivateShield()
    {
        hasShield = true;

        if (shieldVisual != null) shieldVisual.SetActive(true);
        if (shieldUIIcon != null) shieldUIIcon.SetActive(true);

        if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);
        shieldCoroutine = StartCoroutine(ShieldTimeoutRoutine());
    }

    IEnumerator ShieldTimeoutRoutine()
    {
        float timer = shieldDuration;

        // Chạy bình thường đến còn 1 giây
        while (timer > 1f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // Nhấp nháy 1 giây cuối
        while (timer > 0f)
        {
            timer -= 0.15f;

            if (shieldUIIcon != null)
            {
                shieldUIIcon.SetActive(!shieldUIIcon.activeSelf);
            }

            yield return new WaitForSeconds(0.15f);
        }

        // Tắt hẳn
        RemoveShield();
    }

    void RemoveShield()
    {
        hasShield = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (shieldUIIcon != null) shieldUIIcon.SetActive(false);
    }

    // =========================
    // DAMAGE
    // =========================

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        if (hasShield)
        {
            if (shieldCoroutine != null) StopCoroutine(shieldCoroutine);
            RemoveShield();
            StartCoroutine(TemporaryInvincible());
            return;
        }

        HeartHealth heart = GetComponent<HeartHealth>();

        if (heart != null)
        {
            heart.TakeDamage(damage);

            // CHỈ BẤT TỬ KHI NHÂN VẬT CÒN SỐNG (Để tránh lỗi báo coroutine trên object đã bị xóa)
            if (heart.currentHearts > 0)
            {
                StartCoroutine(TemporaryInvincible());
            }
        }
    }

    IEnumerator TemporaryInvincible()
    {
        isInvincible = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        float duration = 0.5f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += 0.1f;
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        if (sr != null) sr.enabled = true;
        isInvincible = false;
    }

    // =========================
    // GRID
    // =========================

    void SnapToGrid()
    {
        if (grid != null)
        {
            transform.position = grid.GetCellCenterWorld(grid.WorldToCell(transform.position));
        }
    }
}