using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthFlyingDemon : MonoBehaviour
{
    [Header("Nyawa")]
    public int nyawaMaksimum = 3;
    private int nyawaSekarang;

    [Header("Komponen")]
    private Animator animator;
    private Rigidbody2D rb;
    private bool sudahMati = false;

    [Header("Flying Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    // Animator parameters
    private static readonly int DieParam = Animator.StringToHash("die");
    private static readonly int HurtParam = Animator.StringToHash("hurt");

    void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        // Pastikan gravity scale 0 untuk mob terbang
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    public void KurangiNyawa()
    {
        if (sudahMati) return;

        nyawaSekarang--;
        Debug.Log("Flying Demon kena! Sisa nyawa: " + nyawaSekarang);

        // Trigger animasi hurt
        if (animator != null)
        {
            animator.SetTrigger("hurt");
        }

        // Knockback effect untuk mob terbang
        StartCoroutine(KnockbackEffect());

        if (nyawaSekarang <= 0)
        {
            Debug.Log("🔥 Flying Demon akan mati!");
            Mati();
        }
    }

    private IEnumerator KnockbackEffect()
    {
        if (rb != null)
        {
            // Hitung arah knockback dari player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector2 knockbackDirection = (transform.position - player.transform.position).normalized;
                rb.velocity = knockbackDirection * knockbackForce;
            }

            yield return new WaitForSeconds(knockbackDuration);

            // Reset velocity setelah knockback
            rb.velocity = Vector2.zero;
        }
    }

    private void Mati()
    {
        sudahMati = true;
        Debug.Log("Flying Demon mati");

        // Matikan collider langsung agar tidak bisa kena hit lagi
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
            Debug.Log("Collider Flying Demon dimatikan");
        }

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
            animator.SetTrigger("die");
            Debug.Log("🎬 Animasi mati dipicu untuk Flying Demon");
        }

        // Hentikan movement
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        
        // Hancurkan setelah animasi selesai (akan dipanggil dari Animation Event)
    }

    // Fungsi ini akan dipanggil lewat Animation Event di akhir animasi mati
    public void Hancurkan()
    {
        Destroy(gameObject);
    }

    // Method untuk mendapatkan status nyawa
    public int GetNyawaSekarang()
    {
        return nyawaSekarang;
    }

    public bool SudahMati()
    {
        return sudahMati;
    }
}
