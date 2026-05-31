using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("===== UI Panels =====")]
    public GameObject aboutPanel;
    public GameObject settingsPanel;

    [Header("===== Audio & Objects =====")]
    public AudioSource backgroundMusic;
    // Nơi gán đối tượng AudioManager độc lập để giữ lại khi qua màn
    public GameObject audioManagerObject;

    // =====================================================
    // AWAKE & START
    // =====================================================

    void Awake()
    {
        // CHỈ giữ lại AudioManager độc lập (nếu có kéo vào)
        // Đã xóa bỏ đoạn 'else' cũ để tránh kéo theo cả cụm Menu nút bấm sang màn 1-1
        if (audioManagerObject != null)
        {
            DontDestroyOnLoad(audioManagerObject);
        }
    }

    void Start()
    {
        // Tự động ẩn các panel khi vừa mở game để menu sạch sẽ
        if (aboutPanel != null)
            aboutPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // =====================================================
    // SETTINGS PANEL FUNCTIONS
    // =====================================================

    // Mở Settings Panel
    public void OpenSettings()
    {
        Debug.Log("Mo bang thong tin Settings!");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // Đóng Settings Panel
    public void CloseSettings()
    {
        Debug.Log("Dong Settings");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // Chức năng nút: BẬT NHẠC (Gán cho nút ảo đè lên chữ BẬT NHẠC)
    public void TurnMusicOn()
    {
        if (backgroundMusic == null) return;

        backgroundMusic.mute = false; // Tắt im lặng -> Phát nhạc
        Debug.Log("Nhạc đã BẬT (Mute = false)");
    }

    // Chức năng nút: TẮT NHẠC (Gán cho nút ảo đè lên chữ TẮT NHẠC)
    public void TurnMusicOff()
    {
        if (backgroundMusic == null) return;

        backgroundMusic.mute = true; // Bật im lặng -> Tắt nhạc
        Debug.Log("Nhạc đã TẮT (Mute = true)");
    }

    // =====================================================
    // PLAY GAME
    // =====================================================

    public void PlayGame()
    {
        Debug.Log("Loading Game Scene 1-1...");
        SceneManager.LoadScene("1-1");
    }

    // =====================================================
    // ABOUT PANEL FUNCTIONS
    // =====================================================

    // Mở About Panel
    public void OpenAbout()
    {
        Debug.Log("Mo About");
        if (aboutPanel != null)
        {
            aboutPanel.SetActive(true);
        }
    }

    // Đóng About Panel
    public void CloseAbout()
    {
        Debug.Log("Dong About");
        if (aboutPanel != null)
        {
            aboutPanel.SetActive(false);
        }
    }

    // =====================================================
    // EXIT GAME
    // =====================================================
    public void QuitGame()
    {
        Debug.Log("Người chơi đã bấm EXIT thoát game!");

#if UNITY_EDITOR
        // Nếu đang chơi thử trong phần mềm Unity, lệnh này sẽ tự nhả nút Play để tắt game luôn
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Lệnh gốc của ông: Tắt hẳn game khi đã build ra file cài đặt (.exe)
        Application.Quit();
#endif
    }
}