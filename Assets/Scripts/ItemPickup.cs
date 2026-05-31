using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType { ExplosionRange, MaxBombs, Shield }

    [Header("Loại Vật Phẩm")]
    public ItemType itemType;

    [Header("Giá trị cộng thêm")]
    public int valueAmount = 1;

    [Header("Cài đặt nhặt đồ")]
    [Tooltip("Khoảng cách tối đa so với tâm vật phẩm mà người chơi có thể nhặt được")]
    public float pickupThreshold = 0.35f;

    // --- MỚI: Tự hủy sau 5 giây ---
    private void Start()
    {
        // Destroy đối tượng sau 5 giây nếu không bị nhặt trước đó
        Destroy(gameObject, 5f);
    }
    // ----------------------------

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            float distance = Vector2.Distance(transform.position, collision.transform.position);

            if (distance <= pickupThreshold)
            {
                PlayerController player = collision.GetComponent<PlayerController>();

                if (player != null)
                {
                    switch (itemType)
                    {
                        case ItemType.ExplosionRange:
                            player.IncreaseExplosionRange(valueAmount);
                            break;

                        case ItemType.MaxBombs:
                            player.IncreaseMaxBombs(valueAmount);
                            break;

                        case ItemType.Shield:
                            player.ActivateShield();
                            break;
                    }

                    // Hủy ngay lập tức khi người chơi nhặt
                    Destroy(gameObject);
                }
            }
        }
    }
}