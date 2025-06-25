using UnityEngine;

public class SistemNyawaMob : MonoBehaviour
{
    [Header("Nyawa")]
    public int nyawaMaksimum = 2;
    private int nyawaSekarang;

    [Header("Komponen")]
    private Animator animator;
    private bool sudahMati = false;

    void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        animator = GetComponent<Animator>();
    }

    public void KurangiNyawa()
    {
        if (sudahMati) return;

        nyawaSekarang--;
        Debug.Log("Mob kena! Sisa nyawa: " + nyawaSekarang);

        if (nyawaSekarang <= 0)
        {
            Mati();
        }
    }

    private void Mati()
    {
        sudahMati = true;
        Debug.Log("Mob mati");

        // Tambah mob ke statistik player
        PlayerStatTracker stat = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerStatTracker>();
        if (stat != null)
        {
            stat.TambahMob();
            Debug.Log("✅ Total mob sekarang: " + stat.totalMob);
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerStatTracker tidak ditemukan!");
        }

        // Set animasi kematian
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }

        // Matikan physics dan collider
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
    }

    // Fungsi ini akan dipanggil lewat Animation Event di akhir animasi mati
    public void Hancurkan()
    {
        Destroy(gameObject);
    }
}
