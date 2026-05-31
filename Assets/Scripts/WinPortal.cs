using UnityEngine;

public class WinPortal : MonoBehaviour
{
    // Hàm này tự chạy khi Player đi xuyên qua cổng dịch chuyển
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem đối tượng chạm vào có phải là người chơi không
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player đã chạm vào cổng Thắng!");

            // Gọi lệnh kích hoạt bảng Thắng cuộc bên GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerWin();
            }
        }
    }
}