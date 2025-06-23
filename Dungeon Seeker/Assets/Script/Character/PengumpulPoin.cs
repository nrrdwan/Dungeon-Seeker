using UnityEngine;

public class PengumpulPoin : MonoBehaviour
{
    public int totalPoin = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Poin"))
        {
            totalPoin++;
            Debug.Log("✨ Poin dikumpulkan! Total: " + totalPoin);
            Destroy(other.gameObject); // Hapus objek poin
        }
    }
}
