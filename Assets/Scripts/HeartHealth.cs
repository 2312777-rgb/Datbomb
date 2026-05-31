using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class HeartHealth : MonoBehaviour
{
    [Header("Cấu hình Máu")]
    public int maxHearts = 3;
    public int currentHearts; // Để public để dễ debug

    [Header("Giao diện UI Trái Tim")]
    public List<Image> heartImages;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    [Header("Thời gian bất tử")]
    public float invincibilityDuration = 1f;
    private float invincibilityTimer = 0f;

    void Awake()
    {
        // Khởi tạo máu ban đầu
        currentHearts = maxHearts;
    }

    void Start()
    {
        InitializeHeartList();
        UpdateHeartUI();
    }

    // Tự động tìm tất cả các ảnh có tên chứa "Heart" trong các object con
    void InitializeHeartList()
    {
        if (heartImages == null || heartImages.Count == 0)
        {
            // Lấy tất cả Image trong các con, bao gồm cả các object đang bị tắt
            heartImages = GetComponentsInChildren<Image>(true)
                .Where(img => img.name.Contains("Heart"))
                .OrderBy(img => img.name) // Sắp xếp theo tên để tim hiện đúng thứ tự
                .ToList();
        }
    }

    void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damageAmount, int bombID = 0)
    {
        // Bỏ qua nếu đang trong thời gian bất tử (tránh mất máu liên tục)
        if (invincibilityTimer > 0) return;

        // Bắt đầu tính thời gian bất tử
        invincibilityTimer = invincibilityDuration;

        // Trừ máu và giới hạn để máu không bị âm
        currentHearts -= damageAmount;
        currentHearts = Mathf.Clamp(currentHearts, 0, maxHearts);

        UpdateHeartUI();

        // Kiểm tra cái chết
        if (currentHearts <= 0)
        {
            Die();
        }
    }

    public void UpdateHeartUI()
    {
        if (heartImages == null || heartImages.Count == 0) return;

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] == null) continue;

            if (i < currentHearts)
            {
                heartImages[i].sprite = fullHeartSprite; // Tim đỏ
                heartImages[i].enabled = true;
            }
            else if (i < maxHearts)
            {
                heartImages[i].sprite = emptyHeartSprite; // Tim xám/đen
                heartImages[i].enabled = true;
            }
            else
            {
                heartImages[i].enabled = false;
            }
        }
    }

    // --- HÀM XỬ LÝ CÁI CHẾT ĐÃ ĐƯỢC HOÀN THIỆN ---
    void Die()
    {
        Debug.Log("Player đã chết!");

        // GỌI GAMEMANAGER ĐỂ HIỆN BẢNG THUA
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }

        // TIÊU DIỆT NHÂN VẬT: Lệnh này sẽ xóa hình ảnh Player khỏi bản đồ
        Destroy(gameObject);
    }
}