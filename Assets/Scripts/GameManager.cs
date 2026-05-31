using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("===== UI Panels =====")]
    public GameObject winPanel;   // Ô kéo giao diện THẮNG (You Win)
    public GameObject losePanel;  // Ô kéo giao diện THUA (Game Over)
    public GameObject soundPanel; // Ô kéo bảng cài đặt âm thanh (Sound Panel)

    [Header("===== Bộ Đôi Nút Tạm Dừng Tự Đổi Kính =====")]
    public GameObject pauseButton;  // Nút hình gạch dọc || 
    public GameObject resumeButton; // Nút hình tam giác ►

    [Header("===== Scene Settings =====")]
    public string nextSceneName;  // Tên màn chơi tiếp theo (Màn Boss thì để trống)

    private void Awake()
    {
        Time.timeScale = 1f; // Luôn cho game chạy bình thường khi vừa load màn
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 1. Mới vào trận: Ẩn sạch các bảng UI để người chơi tập trung đá game
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(false); // Bắt buộc ẩn bảng âm thanh lúc đầu

        // 2. Hiện nút Dừng (||) và ẩn nút Chơi tiếp (►) đi trước
        if (pauseButton != null) pauseButton.SetActive(true);
        if (resumeButton != null) resumeButton.SetActive(false);
    }

    // ========================================================
    // 🛠️ BỘ BA HÀM XỬ LÝ ĐÚNG QUY TRÌNH: PAUSE -> HIỆN BẢNG -> RESUME
    // ========================================================

    // BƯỚC 1: Người chơi bấm nút TẠM DỪNG (Hình ||)
    public void PauseGame()
    {
        // ÉP BUỘC DỪNG GAME TRƯỚC (Đóng băng toàn bộ thời gian, quái, bom đứng im)
        Time.timeScale = 0f;
        Debug.Log("➡️ Bước 1: Đã ĐÓNG BĂNG game (Time.timeScale = 0).");

        // Gọi luồng xử lý Bước 2 để hiện bảng sau khi game đã dừng hẳn
        StartCoroutine(WaitAndShowPanelRoutine());
    }

    // BƯỚC 2: Chờ hệ thống dừng hẳn rồi mới HIỆN BẢNG LÊN
    private IEnumerator WaitAndShowPanelRoutine()
    {
        // Đợi 0.02 giây thời gian thực để Unity xử lý xong lệnh đóng băng
        yield return new WaitForSecondsRealtime(0.02f);

        Debug.Log("➡️ Bước 2: Game đã dừng hoàn toàn -> HIỆN bảng âm thanh lên.");

        if (pauseButton != null) pauseButton.SetActive(false);   // Ẩn nút || đi
        if (resumeButton != null) resumeButton.SetActive(true);  // Hiện nút ► lên
        if (soundPanel != null) soundPanel.SetActive(true);      // CHÍNH THỨC HIỆN BẢNG ÂM THANH
    }

    // BƯỚC 3: Người chơi bấm nút CHƠI TIẾP (Hình ►) hoặc nút ĐÓNG (Close) trên bảng
    public void ResumeGame()
    {
        Debug.Log("➡️ Bước 3: Người chơi bấm tiếp tục -> ẨN bảng âm thanh, cho game chạy lại.");

        if (soundPanel != null) soundPanel.SetActive(false);     // Ẩn bảng âm thanh đi trước
        if (pauseButton != null) pauseButton.SetActive(true);    // Hiện lại nút || ngoài màn hình
        if (resumeButton != null) resumeButton.SetActive(false); // Ẩn nút ► đi

        // Sau khi dọn dẹp giao diện UI xong mới mở băng cho game chạy tiếp
        Time.timeScale = 1f;
    }

    // Hàm bổ trợ cho nút ĐÓNG (Cục màu trắng bo tròn ở góc bảng)
    public void CloseSoundPanel()
    {
        ResumeGame(); // Gọi chung lại hàm ResumeGame để quy trình được khép kín đồng bộ
    }

    // ==========================================
    // 🔊 CÁC HÀM BẬT / TẮT ÂM THANH TỔNG (Trong bảng)
    // ==========================================
    public void TurnOnSound()
    {
        AudioListener.volume = 1f; // Bật 100% âm thanh game
        Debug.Log("🔊 Toàn bộ âm thanh game đã được BẬT!");
    }

    public void TurnOffSound()
    {
        AudioListener.volume = 0f; // Mute hoàn toàn âm thanh game
        Debug.Log("🔇 Toàn bộ âm thanh game đã được TẮT!");
    }

    // ==========================================
    // 🔄 CÁC HÀM ĐIỀU HƯỚNG MÀN CHƠI
    // ==========================================
    // ==========================================
    // 🔄 CÁC HÀM ĐIỀU HƯỚNG MÀN CHƠI
    // ==========================================
    public void CheckWinCondition()
    {
        // Gọi Coroutine để đợi Unity xử lý xong việc xóa (Destroy) quái/boss rồi mới đếm
        StartCoroutine(WaitAndCheckWinRoutine());
    }

    private IEnumerator WaitAndCheckWinRoutine()
    {
        // ĐỢI ĐẾN CUỐI FRAME: Đảm bảo con quái/boss vừa bị giết đã thực sự bốc hơi khỏi Scene
        yield return new WaitForEndOfFrame();

        // 1. Tìm Boss xem còn sống không
        BossMovement boss = Object.FindFirstObjectByType<BossMovement>();

        // 2. Tìm toàn bộ Slime thường và đếm số lượng còn sống
        SlimeMovement[] normalSlimes = Object.FindObjectsByType<SlimeMovement>(FindObjectsSortMode.None);

        int livingNormalSlimes = 0;
        foreach (var slime in normalSlimes)
        {
            if (slime != null && slime.gameObject.activeInHierarchy)
            {
                livingNormalSlimes++;
            }
        }

        // 3. ĐIỀU KIỆN THẮNG CHUẨN XÁC:
        // - boss == null : Tức là Boss đã chết (hoặc màn này vốn dĩ không có Boss)
        // - livingNormalSlimes == 0 : Tức là toàn bộ quái thường đã bị tiêu diệt sạch
        if (boss == null && livingNormalSlimes == 0)
        {
            Debug.Log("🏆 Boss và Quái đã bị dọn sạch. Kích hoạt WIN GAME!");
            TriggerWin();
        }
    }

    public void TriggerGameOver()
    {
        if (losePanel != null) { losePanel.SetActive(true); Time.timeScale = 0f; }
    }

    public void TriggerWin() { StartCoroutine(WaitAndShowWinPanel()); }
    private IEnumerator WaitAndShowWinPanel()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        if (winPanel != null) { winPanel.SetActive(true); Time.timeScale = 0f; }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName)) { Time.timeScale = 1f; SceneManager.LoadScene(nextSceneName); }
    }
}