using UnityEngine;

public class SistemNyawa : MonoBehaviour
{
    public int nyawaMaksimum = 3;
    private int nyawaSekarang;

    public GameObject[] ikonNyawa; // Isi dengan icon hati di UI (opsional)
    public GameObject panelGameOver; // Panel Game Over (jika ada)
    
    // Reference to player movement component for checking immunity status
    private PlayerMovement playerMovement;

    private void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        UpdateUI();
        
        // Get reference to the player movement component
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("SistemNyawa: PlayerMovement component not found on this object!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Penghalang"))
        {
            // Check if player is currently immune to damage (during dodge)
            if (playerMovement != null && playerMovement.IsImmuneToDamage())
            {
                Debug.Log("Player is immune to damage! No life lost.");
                return; // Skip damage if player is currently immune
            }
            
            KurangiNyawa();
        }
    }

    // Modify the OnTriggerStay2D to add a delay between damage applications
    private float lastDamageTime = 0f;
    [SerializeField] private float damageCooldown = 1f; // Seconds between damage applications

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Penghalang"))
        {
            // First check if player is currently immune due to dodge
            if (playerMovement != null && playerMovement.IsImmuneToDamage())
            {
                Debug.Log("Player is immune to damage during dodge! No life lost.");
                return; // Skip damage if player is immune
            }
            
            // Add cooldown between damage applications to prevent rapid health loss
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                KurangiNyawa();
                lastDamageTime = Time.time;
            }
        }
    }

    private void KurangiNyawa()
    {
        // Double-check immunity before actually reducing health
        if (playerMovement != null && playerMovement.IsImmuneToDamage())
        {
            Debug.Log("Final immunity check prevented damage!");
            return;
        }
        
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
