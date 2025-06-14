using UnityEngine;

public class SistemNyawa : MonoBehaviour
{
    public int nyawaMaksimum = 3;
    private int nyawaSekarang;

    public GameObject[] ikonNyawa; // Isi dengan icon hati di UI (opsional)
    public GameObject panelGameOver; // Panel Game Over (jika ada)

    private void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        UpdateUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Penghalang"))
        {
            KurangiNyawa();
        }
    }

    private void KurangiNyawa()
    {
        nyawaSekarang--;

        Debug.Log("Nyawa tersisa: " + nyawaSekarang);
        UpdateUI();

        if (nyawaSekarang <= 0)
        {
            GameOver();
        }
    }

    private void UpdateUI()
    {
        // Jika pakai icon hati, update aktif/tidaknya
        for (int i = 0; i < ikonNyawa.Length; i++)
        {
            if (i < nyawaSekarang)
                ikonNyawa[i].SetActive(true);
            else
                ikonNyawa[i].SetActive(false);
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        // Bisa freeze player, munculkan UI, restart, dsb
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }

        // Contoh freeze player:
        GetComponent<PlayerMovement>().enabled = false;
        Time.timeScale = 0f; // (opsional) berhentikan waktu
    }
}
