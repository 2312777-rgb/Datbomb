using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    [Header("Explosion Settings")]
    public GameObject explosionPrefab;
    public float explodeDelay = 4f;
    public bool shakeEffect = true;
    public float shakeSpeed = 20f;
    public float shakeAmount = 0.05f;

    [Header("Audio Settings")]
    public AudioClip explosionSound;

    [Header("Bomb Logic")]
    [HideInInspector]
    public int explosionRange = 2;
    public LayerMask solidLayer;
    public LayerMask brickLayer;
    public LayerMask damageLayer;
    public int damageAmount = 1;

    [Header("Item Spawner Settings")]
    [Range(0f, 100f)]
    public float itemDropChance = 30f;
    public GameObject[] itemsToDrop;

    private bool exploded = false;
    private Vector3 originalScale;
    private Tilemap brickTilemap;
    private Grid grid;

    private List<HeartHealth> damagedPlayers = new List<HeartHealth>();
    private List<GameObject> damagedEnemies = new List<GameObject>();

    void Start()
    {
        grid = FindAnyObjectByType<Grid>();
        SnapToGrid();
        originalScale = transform.localScale;

        GameObject brickObj = GameObject.Find("Brick");
        if (brickObj != null)
        {
            brickTilemap = brickObj.GetComponent<Tilemap>();
        }

        // TỰ ĐỘNG ĐỒNG BỘ: Lấy tầm nổ thực tế từ PlayerController hiện tại
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            explosionRange = player.explosionRange;
        }

        // PHÁT ÂM THANH NGAY KHI VỪA ĐẶT BOMB XUỐNG
        if (explosionSound != null)
        {
            AudioListener listener = Object.FindFirstObjectByType<AudioListener>();
            if (listener != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, listener.transform.position);
            }
            else
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            }
        }

        Invoke(nameof(Explode), explodeDelay);
    }

    void Update()
    {
        if (!exploded && shakeEffect)
        {
            float scale = 1 + Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
            transform.localScale = originalScale * scale;
        }
    }

    void SnapToGrid()
    {
        if (grid == null) grid = FindAnyObjectByType<Grid>();
        if (grid != null)
        {
            Vector3Int cellPos = grid.WorldToCell(transform.position);
            transform.position = grid.GetCellCenterWorld(cellPos);
        }
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        SnapToGrid();
        Vector3 originPos = transform.position;

        damagedPlayers.Clear();
        damagedEnemies.Clear();

        // 1. NỔ TẠI TÂM BOM
        CreateExplosion(originPos, Quaternion.identity);
        DamageEntitiesAtPosition(originPos);

        // 2. BẮN TIA QUÉT THEO 4 HƯỚNG Ô CỜ
        FireExplosionRay(originPos, Vector2.up, 90f);
        FireExplosionRay(originPos, Vector2.down, 270f);
        FireExplosionRay(originPos, Vector2.left, 180f);
        FireExplosionRay(originPos, Vector2.right, 0f);

        Destroy(gameObject);
    }

    void FireExplosionRay(Vector3 origin, Vector2 direction, float angle)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector3 currentCheckPos = origin;

        for (int i = 1; i <= explosionRange; i++)
        {
            Vector3 targetPos = origin + new Vector3(direction.x * i, direction.y * i, 0);
            float stepDistance = 1f;

            // 1. Kiểm tra tường đá cứng trước (Solid) bằng Raycast (Tường đứng yên nên xài tia tĩnh là đủ)
            RaycastHit2D hitSolid = Physics2D.Raycast(currentCheckPos, direction, stepDistance, solidLayer);
            if (hitSolid.collider != null)
            {
                return; // Gặp tường cứng -> Lập tức chặn tia nổ
            }

            // 2. Kiểm tra tường gạch gỗ phá hủy được (Brick)
            RaycastHit2D hitBrick = Physics2D.Raycast(currentCheckPos, direction, stepDistance, brickLayer);
            if (hitBrick.collider != null)
            {
                Vector3Int brickCell = grid.WorldToCell(targetPos);

                if (brickTilemap != null && brickTilemap.HasTile(brickCell))
                {
                    brickTilemap.SetTile(brickCell, null);
                    CreateExplosion(targetPos, rotation);
                    DamageEntitiesAtPosition(targetPos);
                    TrySpawnItem(targetPos);
                }
                else
                {
                    CreateExplosion(targetPos, rotation);
                    DamageEntitiesAtPosition(targetPos);
                }

                return; // Gặp gạch -> Lập tức chặn tia nổ
            }

            // 3. KHẮC PHỤC TRIỆT ĐỂ: Dùng BoxCastAll thay cho RaycastAll
            // Quét một hình hộp vuông kích thước 0.9x0.9 (gần bằng 1 ô) để quét sạch không chừa khe hở nào
            RaycastHit2D[] hitsEnemy = Physics2D.BoxCastAll(currentCheckPos, new Vector2(0.9f, 0.9f), 0f, direction, stepDistance, damageLayer);
            bool hasEnemy = false;
            RaycastHit2D enemyHitInfo = default;

            foreach (RaycastHit2D hit in hitsEnemy)
            {
                // Chỉ bắt các vật thể có Tag "Enemy", lọc bỏ Player/Quả bom đứng trùng vị trí
                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    hasEnemy = true;
                    enemyHitInfo = hit;
                    break;
                }
            }

            if (hasEnemy && enemyHitInfo.collider != null)
            {
                CreateExplosion(targetPos, rotation);

                // Trực tiếp gây sát thương cho quái vật chặn tia nổ
                GameObject enemyRoot = enemyHitInfo.collider.transform.root.gameObject;
                if (!damagedEnemies.Contains(enemyRoot))
                {
                    damagedEnemies.Add(enemyRoot);

                    BossMovement boss = enemyRoot.GetComponent<BossMovement>() ?? enemyHitInfo.collider.GetComponent<BossMovement>();
                    if (boss != null)
                    {
                        boss.TakeDamage(damageAmount);
                    }
                    else
                    {
                        Destroy(enemyRoot);
                    }
                }

                return; // GẶP QUÁI VẬT LÀ LẬP TỨC CHẶN TIA NỔ HẾT ĐƯỜNG ĐI XUYÊN
            }

            // 4. Nếu là ô trống bình thường -> Tạo lửa, gây sát thương và tiếp tục quét ô sau
            CreateExplosion(targetPos, rotation);
            DamageEntitiesAtPosition(targetPos);

            currentCheckPos = targetPos;
        }
    }
    void TrySpawnItem(Vector3 spawnPosition)
    {
        if (itemsToDrop == null || itemsToDrop.Length == 0) return;

        if (Random.Range(0f, 100f) <= itemDropChance)
        {
            int randomIndex = Random.Range(0, itemsToDrop.Length);
            GameObject selectedItem = itemsToDrop[randomIndex];

            if (selectedItem != null)
            {
                Instantiate(selectedItem, spawnPosition, Quaternion.identity);
            }
        }
    }

    void DamageEntitiesAtPosition(Vector3 position)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, new Vector2(0.9f, 0.9f), 0f, damageLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponent<TilemapCollider2D>() != null || hit.GetComponent<CompositeCollider2D>() != null)
                continue;

            // Xử lý Người chơi
            HeartHealth playerHealth = hit.GetComponent<HeartHealth>() ?? hit.GetComponentInParent<HeartHealth>();
            if (playerHealth != null && !damagedPlayers.Contains(playerHealth))
            {
                damagedPlayers.Add(playerHealth);
                PlayerController pController = playerHealth.GetComponent<PlayerController>();
                if (pController != null) pController.TakeDamage(damageAmount);
                else playerHealth.TakeDamage(damageAmount, GetInstanceID());
                continue;
            }

            // Xử lý Quái vật & Boss (đảm bảo không lọt sát thương ở tâm nổ)
            if (hit.CompareTag("Enemy"))
            {
                GameObject enemyRoot = hit.transform.root.gameObject;
                if (!damagedEnemies.Contains(enemyRoot))
                {
                    damagedEnemies.Add(enemyRoot);

                    BossMovement boss = enemyRoot.GetComponent<BossMovement>() ?? hit.GetComponent<BossMovement>();

                    if (boss != null)
                    {
                        boss.TakeDamage(damageAmount);
                    }
                    else
                    {
                        Destroy(enemyRoot);
                    }
                }
            }
        }
    }

    void CreateExplosion(Vector3 position, Quaternion rotation)
    {
        if (explosionPrefab == null) return;

        GameObject explosion = Instantiate(explosionPrefab, position, rotation);
        explosion.transform.localScale = Vector3.one;
        Destroy(explosion, 0.8f);
    }
}