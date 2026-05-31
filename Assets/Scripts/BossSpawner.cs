using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Minion Settings")]
    public GameObject minionPrefab;   // Kéo thả Prefab quái con vào đây
    public float spawnInterval = 5f;   // Thời gian giãn cách giữa mỗi lần đẻ quái (5 giây)
    public int maxMinions = 6;         // Giới hạn số lượng quái tối đa trên map để tránh lag máy

    [Header("Spawn Position")]
    public float spawnRadius = 1f;     // Bán kính xung quanh Boss để gọi quái (tránh bị đè lên tâm Boss)

    void Start()
    {
        // Hàm này sẽ tự động gọi hàm "SpawnMinion" sau 5 giây đầu tiên, và lặp lại mỗi 5 giây sau đó
        InvokeRepeating(nameof(SpawnMinion), spawnInterval, spawnInterval);
    }

    void SpawnMinion()
    {
        // 1. Kiểm tra xem trên Map hiện tại đang có bao nhiêu quái con
        // (Tìm theo Tag "Enemy", nếu bạn đặt tag khác thì sửa lại nhé)
        GameObject[] currentEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Nếu số lượng quái vượt quá giới hạn -> Nghỉ đẻ, chờ người chơi tiêu diệt bớt
        if (currentEnemies.Length >= maxMinions)
        {
            return;
        }

        // 2. Tính toán một vị trí ngẫu nhiên xung quanh Boss để quái con xuất hiện
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRadius, spawnRadius),
            Random.Range(-spawnRadius, spawnRadius),
            0
        );
        Vector3 spawnPosition = transform.position + randomOffset;

        // 3. Tiến hành tạo quái con
        if (minionPrefab != null)
        {
            GameObject minion = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);

            // (Tùy chọn) Đảm bảo quái con vừa sinh ra cũng có Tag là Enemy để dính bom
            minion.tag = "Enemy";
        }
    }

    // Hàm này tự động chạy nếu Boss bị tiêu diệt -> Ngừng đẻ quái hoàn toàn
    void OnDestroy()
    {
        CancelInvoke(nameof(SpawnMinion));
    }
}