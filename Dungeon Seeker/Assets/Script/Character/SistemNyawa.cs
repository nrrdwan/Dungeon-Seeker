using UnityEngine;

public class SistemNyawa : MonoBehaviour
{
    public int nyawaMaksimum = 3;
    private int nyawaSekarang;

    public GameObject[] ikonNyawa;
    public GameObject panelGameOver;
 
    private PlayerMovement playerMovement;

    [Header("Sound Effect")]
    public AudioSource audioSource;         // Drag AudioSource dari inspector
    public AudioClip damageClip;            // Suara saat nyawa berkurang

    private void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        UpdateUI();

        playerMovement = GetComponent<PlayerMovement>();

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
            Destroy(other.gameObject);
        }
    }

    public void KurangiNyawa()
    {
        nyawaSekarang--;

        // 🔊 Mainkan suara damage kalau tersedia
        if (audioSource != null && damageClip != null)
        {
            audioSource.PlayOneShot(damageClip);
        }

        Debug.Log("❤️ Nyawa tersisa: " + nyawaSekarang);
        UpdateUI();

        if (nyawaSekarang <= 0)
        {
            GameOver();
        }
    }

    private void UpdateUI()
    {
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
}
