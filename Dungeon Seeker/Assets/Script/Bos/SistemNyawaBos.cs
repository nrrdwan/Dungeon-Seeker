using UnityEngine;

public class SistemNyawaBos : MonoBehaviour
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
        Debug.Log("Bos kena! Sisa nyawa: " + nyawaSekarang);

        if (nyawaSekarang <= 0)
        {
            Mati();
        }
    }

    private void Mati()
    {
        sudahMati = true;
        Debug.Log("Bos mati");

        if (animator != null)
        {
            animator.SetBool("bosmati", true);
        }

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;

        // 🔥 Fallback pasti jalan setelah animasi
        Invoke(nameof(Hancurkan), 1.3f); // ganti durasi sesuai animasi
    }

    // Fungsi ini akan dipanggil lewat Animation Event di akhir animasi mati
    public void Hancurkan()
    {
        Debug.Log("💥 Fungsi Hancurkan() dipanggil");

        // ❌ Hentikan animator (opsional kalau mau efek diam dulu)
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        // ✅ Hancurkan gameObject
        Destroy(gameObject);
    }
}
