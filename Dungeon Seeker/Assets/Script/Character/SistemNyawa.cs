using UnityEngine;

public class SistemNyawa : MonoBehaviour
{
    public int nyawaMaksimum = 3;
    private int nyawaSekarang;

    public GameObject[] ikonNyawa; // Icon hati (optional)
    public GameObject panelGameOver; // Panel Game Over (optional)

    private PlayerMovement playerMovement;

    private void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        UpdateUI();

        playerMovement = GetComponent<PlayerMovement>();

        // 🔒 Sembunyikan panel game over di awal
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        if (playerMovement == null)
        {
            Debug.LogWarning("SistemNyawa: PlayerMovement tidak ditemukan!");
        }
    }

   private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Penghalang"))
        {
            if (playerMovement != null && playerMovement.IsImmuneToDamage())
            {
                Debug.Log("Player sedang imun saat dodge, tidak kena damage.");
                return;
            }

            KurangiNyawa();
        }
        else if (other.CompareTag("Hati"))
        {
            TambahNyawa();
            Destroy(other.gameObject); // Hapus hati setelah diambil
        }
    }

    private void KurangiNyawa()
    {
        nyawaSekarang--;

        Debug.Log("❤️ Nyawa tersisa: " + nyawaSekarang);
        UpdateUI();

        if (nyawaSekarang <= 0)
        {
            GameOver();
        }
    }

    private void UpdateUI()
    {
        // Update ikon nyawa
        for (int i = 0; i < ikonNyawa.Length; i++)
        {
            ikonNyawa[i].SetActive(i < nyawaSekarang);
        }
    }

    private void GameOver()
    {
        Debug.Log("☠️ Game Over!");

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Time.timeScale = 0f; // Hentikan waktu (opsional)
    }

    private void TambahNyawa()
    {
        if (nyawaSekarang < nyawaMaksimum)
        {
            nyawaSekarang++;
            Debug.Log("💖 Nyawa bertambah! Total: " + nyawaSekarang);
            UpdateUI();
        }
        else
        {
            Debug.Log("Nyawa sudah maksimum.");
        }
    }
}
