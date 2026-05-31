using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float lifeTime = 0.5f;

    [Header("Audio Settings")]
    public AudioClip explosionSound;

    void Start()
    {
        if (explosionSound != null)
        {
            // Tự động tìm chiếc Audio Listener có sẵn trên Main Camera của bạn
            AudioListener listener = Object.FindFirstObjectByType<AudioListener>();

            if (listener != null)
            {
                // Phát âm thanh trực tiếp vào "tai nghe" để biến thành âm thanh 2D rõ ràng
                AudioSource.PlayClipAtPoint(explosionSound, listener.transform.position);
            }
            else
            {
                // Phương án dự phòng nếu không tìm thấy
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            }
        }

        // Hủy hiệu ứng nổ sau khoảng thời gian lifeTime
        Destroy(gameObject, lifeTime);
    }
}