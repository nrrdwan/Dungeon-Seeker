using UnityEngine;

public class PengumpulPoin : MonoBehaviour
{
    private PlayerStatTracker statTracker;

    private void Start()
    {
        statTracker = GetComponent<PlayerStatTracker>();
        if (statTracker == null)
        {
            Debug.LogWarning("⚠️ Tidak ditemukan PlayerStatTracker di Player!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Poin"))
        {
            statTracker?.TambahPoin(1); // Tambah 1 poin ke stat
            Debug.Log("✨ Poin dikumpulkan! Total: " + statTracker.totalPoin);
            Destroy(other.gameObject); // Hapus poin
        }
    }
}
