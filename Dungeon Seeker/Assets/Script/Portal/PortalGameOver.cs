using UnityEngine;
using UnityEngine.UI;

public class PortalGameOver : MonoBehaviour
{
    [Header("Drag & Drop")]
    public GameObject gameOverPanel;
    public Text textPoin; // Ganti TMP_Text jadi Text
    public Text textMob;

    [Header("Sumber Data")]
    public PlayerStatTracker playerStats;

    private bool gameEnded = false;

    void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void Start()
    {
        // 🔒 Pastikan panel disembunyikan di awal
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gameEnded) return;

        if (other.CompareTag("Player"))
        {
            gameEnded = true;
            ShowGameOverPanel();
        }
    }

    void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (textPoin != null && playerStats != null)
            textPoin.text = "Poin: " + playerStats.totalPoin;

        if (textMob != null && playerStats != null)
            textMob.text = "Mob: " + playerStats.totalMob;

        Time.timeScale = 0f;
    }
}
