using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float moveSpeed = 1f;
    public float changeDirectionTime = 3f;

    [Header("4 Ảnh hướng di chuyển")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    [Header("Cấu hình Máu Boss (1 máu = 1 ô)")]
    public BossHealthBar healthBar;
    public float maxHealth = 18f;
    private float currentHealth;

    [Header("Cấu hình Bất tử (Mới)")]
    public float invincibilityDuration = 1f; // Thời gian bất tử (1 giây)
    private float invincibilityTimer;        // Bộ đếm thời gian bất tử

    [Header("Sát thương khi chạm Player (Mới)")]
    public int contactDamage = 1;            // Lượng máu Player bị trừ khi Boss húc phải

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;
    private float directionTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0f;
#else
        rb.drag = 0f;
#endif

        ChooseRandomDirection();

        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.SetupHealthBar(maxHealth);
        }
    }

    void Update()
    {
        // XỬ LÝ THỜI GIAN BẤT TỬ & HIỆU ỨNG NHẤP NHÁY
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;

            // Tạo hiệu ứng chớp tắt liên tục để biết Boss đang bất tử
            float flash = Mathf.PingPong(Time.time * 15f, 1f);
            spriteRenderer.color = new Color(1f, 1f, 1f, flash > 0.5f ? 1f : 0.2f);
        }
        else
        {
            // Trả lại màu sắc đậm rõ bình thường khi hết bất tử
            spriteRenderer.color = Color.white;
        }

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            ChooseRandomDirection();
        }

        UpdateBossSprite();
    }

    void FixedUpdate()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = moveDirection * moveSpeed;
#else
        rb.velocity = moveDirection * moveSpeed;
#endif
    }

    void ChooseRandomDirection()
    {
        int randomChoice = Random.Range(0, 4);
        switch (randomChoice)
        {
            case 0: moveDirection = Vector2.up; break;
            case 1: moveDirection = Vector2.down; break;
            case 2: moveDirection = Vector2.left; break;
            case 3: moveDirection = Vector2.right; break;
        }
        directionTimer = Random.Range(1.5f, changeDirectionTime);
    }

    void UpdateBossSprite()
    {
        if (moveDirection == Vector2.up && spriteUp != null) spriteRenderer.sprite = spriteUp;
        else if (moveDirection == Vector2.down && spriteDown != null) spriteRenderer.sprite = spriteDown;
        else if (moveDirection == Vector2.left && spriteLeft != null) spriteRenderer.sprite = spriteLeft;
        else if (moveDirection == Vector2.right && spriteRight != null) spriteRenderer.sprite = spriteRight;
    }

    // XỬ LÝ VA CHẠM: Khi Boss chạm vào các vật thể khác
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // CHI TIẾT MỚI: KIỂM TRA NẾU CHẠM TRÚNG NHÂN VẬT (PLAYER)
        if (collision.gameObject.CompareTag("Player"))
        {
            // Tìm và gây sát thương trực tiếp lên hệ thống máu của Player
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(contactDamage);
            }
            else
            {
                HeartHealth playerHealth = collision.gameObject.GetComponent<HeartHealth>() ?? collision.gameObject.GetComponentInParent<HeartHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(contactDamage, GetInstanceID());
                }
            }
            // Giải thích: Tại đây CHỈ Player gọi hàm mất máu. Không hề có lệnh trừ máu Boss!
        }

        // Logic bẻ lái tự động khi va chạm tường/thùng cờ cũ của bạn:
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector2.zero;
#else
        rb.velocity = Vector2.zero;
#endif
        ChooseRandomDirection();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
#if UNITY_6000_0_OR_NEWER
        float currentSpeed = rb.linearVelocity.magnitude;
#else
        float currentSpeed = rb.velocity.magnitude;
#endif
        if (currentSpeed < 0.1f)
        {
            ChooseRandomDirection();
        }
    }

    // --- HÀM NHẬN SÁT THƯƠNG (ĐÃ THÊM CƠ CHẾ BẤT TỬ) ---
    // --- HÀM NHẬN SÁT THƯƠNG (ĐÃ THÊM CƠ CHẾ BẤT TỬ) ---
    public void TakeDamage(float damage)
    {
        // Nếu Boss đang trong thời gian bất tử -> Bỏ qua, không nhận bất kỳ sát thương nào!
        if (invincibilityTimer > 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth);
        }

        // KÍCH HOẠT THỜI GIAN BẤT TỬ NGAY KHI VỪA TRÚNG ĐÒN
        invincibilityTimer = invincibilityDuration;

        if (currentHealth <= 0)
        {
            Debug.Log("Boss đã chết! Gọi màn hình Thắng...");

            // 1. Tạm ẩn Boss khỏi màn hình ngay lập tức để hệ thống biết nó đã biến mất
            gameObject.SetActive(false);

            // 2. Gọi thẳng hàm Thắng trong GameManager
            if (GameManager.Instance != null)
            {
                // Nếu luật game là Giết Boss -> Thắng luôn (kể cả còn quái nhỏ):
                GameManager.Instance.TriggerWin();

                // Lưu ý: Nếu luật game của bạn bắt buộc phải giết Boss VÀ dọn sạch cả quái nhỏ thì
                // hãy xóa dòng TriggerWin() ở trên và bỏ // ở dòng dưới đây:
                // GameManager.Instance.CheckWinCondition();
            }

            // 3. Xóa hoàn toàn Boss khỏi bộ nhớ
            Destroy(gameObject);
        }
    }
}