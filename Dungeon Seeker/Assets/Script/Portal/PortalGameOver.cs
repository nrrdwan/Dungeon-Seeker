using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PortalGameOver : MonoBehaviour
{
    [Header("Drag & Drop")]
    public GameObject gameOverPanel;
    public TMP_Text textPoin;
    public TMP_Text textMob;

    [Header("Sumber Data")]
    public PlayerStatTracker playerStats;

    private bool gameEnded = false;

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

        if (textPoin != null)
            textPoin.text = "Poin: " + playerStats.totalPoin;

        if (textMob != null)
            textMob.text = "Mob: " + playerStats.totalMob;

        Time.timeScale = 0f;
    }
}
