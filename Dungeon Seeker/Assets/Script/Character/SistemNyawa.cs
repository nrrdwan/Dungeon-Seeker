using UnityEngine;

public class SistemNyawa : MonoBehaviour
{
    [Header("Konfigurasi Nyawa")]
    public int nyawaMaksimum = 3;
    private int nyawaSekarang;

    [Header("Ikon UI")]
    public GameObject[] ikonNyawa;
    public GameObject panelGameOver;

    [Header("Komponen Tambahan")]
    private PlayerMovement playerMovement;
    private PlayerRespawn playerRespawn;
    private Animator animator;

    [Header("Sound Effect")]
    public AudioSource audioSource;
    public AudioClip damageClip;

    [Header("Sistem Mati Maksimal")]
    private int sisaKesempatanRespawn = 3; // Player hanya bisa respawn ke checkpoint 2 kali

    private void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        UpdateUI();

        playerMovement = GetComponent<PlayerMovement>();
        playerRespawn = GetComponent<PlayerRespawn>();
        animator = GetComponent<Animator>();

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        if (playerMovement == null)
            Debug.LogWarning("SistemNyawa: PlayerMovement tidak ditemukan!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PenghalangMematikan"))
        {
            Debug.Log("☠️ Terkena penghalang mematikan! Langsung mati.");
            InstanMati();
        }
        else if (other.CompareTag("Penghalang"))
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
            Destroy(other.gameObject);
        }
    }

    public void KurangiNyawa()
    {
        nyawaSekarang--;

        if (audioSource != null && damageClip != null)
        {
            audioSource.PlayOneShot(damageClip);
        }

        if (animator != null)
        {
            animator.SetTrigger("hurt");
        }

        Debug.Log("❤️ Nyawa tersisa: " + nyawaSekarang);
        UpdateUI();

        if (nyawaSekarang <= 0)
        {
            if (sisaKesempatanRespawn > 0)
            {
                sisaKesempatanRespawn--;
                Debug.Log("🔁 Respawn ke checkpoint! Sisa respawn: " + sisaKesempatanRespawn);
                RespawnKeCheckpoint();
            }
            else
            {
                GameOver();
            }
        }
    }

    private void RespawnKeCheckpoint()
    {
        nyawaSekarang = nyawaMaksimum;
        UpdateUI();

        if (playerRespawn != null)
        {
            playerRespawn.Respawn();
        }

        if (animator != null)
        {
            animator.ResetTrigger("die");
        }

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < ikonNyawa.Length; i++)
        {
            if (ikonNyawa[i] != null)
            {
                ikonNyawa[i].SetActive(i < nyawaSekarang);
            }
            else
            {
                Debug.LogWarning($"❗ ikonNyawa[{i}] is null. Cek di Inspector!");
            }
        }
    }

    private void GameOver()
    {
        Debug.Log("☠️ GAME OVER setelah 3x mati!");

        if (animator != null)
        {
            animator.SetTrigger("die");
        }

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        StartCoroutine(GameOverDelay());
    }

    private System.Collections.IEnumerator GameOverDelay()
    {
        yield return new WaitForSeconds(1f);
        Time.timeScale = 0f;
    }

    public void TambahNyawa()
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

    public void InstanMati()
    {
        nyawaSekarang = 0;
        UpdateUI();

        if (sisaKesempatanRespawn > 0)
        {
            sisaKesempatanRespawn--;
            RespawnKeCheckpoint();
        }
        else
        {
            GameOver();
        }
    }
    public void ResetNyawa()
    {
        nyawaSekarang = nyawaMaksimum;
        UpdateUI();
    }

    public void ResetForRetry()
    {
        // Reset semua state untuk retry
        nyawaSekarang = nyawaMaksimum;
        sisaKesempatanRespawn = 3;

        // Aktifkan kembali PlayerMovement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Reset animator
        if (animator != null)
        {
            animator.ResetTrigger("die");
            animator.ResetTrigger("hurt");
        }

        // Sembunyikan panel game over
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        // Reset time scale
        Time.timeScale = 1f;
        
        // Pindahkan player ke posisi spawn jika ada PlayerRespawn
        if (playerRespawn != null)
        {
            playerRespawn.Respawn();
        }

        UpdateUI();

        Debug.Log("🔄 Player direset untuk retry!");
    }

}
