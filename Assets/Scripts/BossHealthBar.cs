using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("Kéo tất cả các ô Fill_Image (màu đỏ) vào đây từ TRÁI sang PHẢI")]
    public List<Image> cellFills = new List<Image>();

    // Hàm này được BossMovement gọi khi bắt đầu game để bật đầy các ô
    public void SetupHealthBar(float maxHealth)
    {
        for (int i = 0; i < cellFills.Count; i++)
        {
            if (cellFills[i] != null)
            {
                cellFills[i].fillAmount = 1f; // Đặt toàn bộ các ô đỏ hiển thị đầy 100%
            }
        }
    }

    // LOGIC MỚI: 1 Máu = 1 Ô
    public void UpdateHealth(float currentHealth)
    {
        // Làm tròn số máu hiện tại để so sánh với danh sách ô cờ (Index)
        int currentHP = Mathf.RoundToInt(currentHealth);

        for (int i = 0; i < cellFills.Count; i++)
        {
            if (cellFills[i] == null) continue;

            // Nếu vị trí của ô (i) nhỏ hơn lượng máu hiện hành -> Bật ô đó lên
            // Ví dụ: Máu = 17 -> Các ô từ 0 đến 16 sẽ bật (đúng 17 ô), ô 17 sẽ tắt.
            if (i < currentHP)
            {
                cellFills[i].fillAmount = 1f; // Hiển thị màu đỏ
            }
            else
            {
                cellFills[i].fillAmount = 0f; // Tắt hoàn toàn màu đỏ của ô này
            }
        }
    }
}