using UnityEngine;

public class SlimeMovement : MonoBehaviour
{
    [Header("Cấu hình di chuyển")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;

    [Header("Trí tuệ nhân tạo (AI)")]
    public float detectionRadius = 5f;
    public LayerMask playerLayer;
    public float changeDirInterval = 3f;

    [Header("4 Ảnh hướng di chuyển")]
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    [Header("Sát thương của Slime")]
    public int heartDamage = 1;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;
    private Vector2 movementDirection;

    private float patrolTimer;
    private bool isChasing = false;
    private int currentDirectionIndex = -1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Tương thích đa phiên bản Unity
#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0f;
#else
        rb.drag = 0f;
#endif

        PickRandomPatrolDirection();
    }

    void Update()
    {
        DetectPlayer();
        UpdateSpriteDirection();
    }

    void FixedUpdate()
    {
        if (isChasing && playerTransform != null)
        {
            Vector2 targetDirection = (playerTransform.position - transform.position).normalized;
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = targetDirection * chaseSpeed;
#else
            rb.velocity = targetDirection * chaseSpeed;
#endif
        }
        else
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = movementDirection * patrolSpeed;
#else
            rb.velocity = movementDirection * patrolSpeed;
#endif

            patrolTimer += Time.fixedDeltaTime;
            if (patrolTimer >= changeDirInterval)
            {
                PickRandomPatrolDirection();
            }
        }
    }

    void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (hit != null)
        {
            if (!isChasing)
            {
                isChasing = true;
                playerTransform = hit.transform;
            }
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                playerTransform = null;
                PickRandomPatrolDirection();
            }
        }
    }

    void PickRandomPatrolDirection()
    {
        patrolTimer = 0f;
        int lastDir = currentDirectionIndex;

        while (currentDirectionIndex == lastDir)
        {
            currentDirectionIndex = Random.Range(0, 4);
        }

        switch (currentDirectionIndex)
        {
            case 0: movementDirection = Vector2.up; break;
            case 1: movementDirection = Vector2.down; break;
            case 2: movementDirection = Vector2.left; break;
            case 3: movementDirection = Vector2.right; break;
        }
    }

    void UpdateSpriteDirection()
    {
#if UNITY_6000_0_OR_NEWER
        Vector2 velocity = rb.linearVelocity;
#else
        Vector2 velocity = rb.velocity;
#endif

        if (velocity.magnitude < 0.1f) return;

        if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
        {
            if (velocity.x > 0 && spriteRight != null) spriteRenderer.sprite = spriteRight;
            else if (velocity.x < 0 && spriteLeft != null) spriteRenderer.sprite = spriteLeft;
        }
        else
        {
            if (velocity.y > 0 && spriteUp != null) spriteRenderer.sprite = spriteUp;
            else if (velocity.y < 0 && spriteDown != null) spriteRenderer.sprite = spriteDown;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // NẾU ĐỤNG TRÚNG TRÁI BOM HOẶC ĐƯỜNG ĐI BỊ CẢN, LẬP TỨC ĐỔI HƯỚNG ĐI TUẦN
        if (!isChasing || collision.gameObject.CompareTag("Bomb"))
        {
            PickRandomPatrolDirection();
        }

        // CHỈ GỌI SÁT THƯƠNG QUA PLAYER CONTROLLER
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(heartDamage);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
#if UNITY_6000_0_OR_NEWER
        float currentSpeed = rb.linearVelocity.magnitude;
#else
        float currentSpeed = rb.velocity.magnitude;
#endif
        // Nếu bị kẹt vào bom hoặc vật thể khác khiến tốc độ giảm, bắt đổi hướng tiếp
        if (currentSpeed < 0.1f)
        {
            PickRandomPatrolDirection();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // =======================================================
    // PHẦN CODE THÊM VÀO: XỬ LÝ CÁI CHẾT & ĐẾM QUÁI ĐỂ PHÂN THẮNG BẠI
    // =======================================================

    // Hàm này để script Quả Bom gọi khi nổ trúng Slime (nếu bom của bạn có gọi hàm)
    public void Die()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Kiểm tra an toàn: Tránh tự kích hoạt khi tắt game hoặc đổi màn chơi thủ công
        if (!gameObject.scene.isLoaded) return;

        // Quét toàn bộ map để tìm tất cả các Object đang gắn script SlimeMovement
        SlimeMovement[] remainingSlimes = FindObjectsByType<SlimeMovement>(FindObjectsSortMode.None);

        int otherSlimesCount = 0;
        foreach (SlimeMovement slime in remainingSlimes)
        {
            // Chỉ đếm những con quái khác (không tính chính bản thân con đang bị xóa này)
            if (slime != null && slime != this)
            {
                otherSlimesCount++;
            }
        }

        // Nếu không còn bất kỳ con quái nào khác sống sót -> Người chơi THẮNG!
        if (otherSlimesCount == 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CheckWinCondition();
            }
        }
    }
}