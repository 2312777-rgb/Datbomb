using UnityEngine;

public class BombSpawner : MonoBehaviour
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab;

    // Lưu trữ quả bom đang hoạt động trên bản đồ
    private GameObject activeBomb = null;
    private Grid grid;

    void Start()
    {
        grid = FindAnyObjectByType<Grid>();
        if (grid == null)
        {
            Debug.LogError("Không tìm thấy Grid trong Scene!");
        }
    }

    void Update()
    {
        // Nhấn nút SPACE để đặt bom
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnBomb();
        }
    }

    void SpawnBomb()
    {
        if (grid == null || bombPrefab == null) return;

        // CHẶN ĐẶT BOM: Nếu quả bom cũ vẫn còn tồn tại (chưa nổ), không cho đặt thêm quả mới!
        if (activeBomb != null)
        {
            Debug.Log("Bom cũ chưa nổ! Bạn không thể đặt thêm bom lúc này.");
            return;
        }

        // Tính toán vị trí tâm ô lưới
        Vector3Int cellPos = grid.WorldToCell(transform.position);
        Vector3 centerPos = grid.GetCellCenterWorld(cellPos);

        // Sinh quả bom và GÁN nó vào biến activeBomb để quản lý khóa đặt bom
        activeBomb = Instantiate(bombPrefab, centerPos, Quaternion.identity);
    }
}   