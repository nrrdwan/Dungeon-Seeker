using UnityEngine;

public class HealthBoss2 : MonoBehaviour
{
    [Header("Nyawa")]
    public int nyawaMaksimum = 100;
    private int nyawaSekarang;
    private int nyawaSebelumnya;

    [Header("Komponen")]
    private Animator animator;
    private bool sudahMati = false;

    [Header("UI Health Bar")]
    public HealthBar healthBar;  // 💡 Tambahkan komponen ini di Inspector

    [Header("Prefab Item Drop")]
    public GameObject prefabHati;
    public GameObject prefabCrystal;

    [Header("Portal")]
    public GameObject portalSaatBosMati;

    private static readonly int DieParam = Animator.StringToHash("die");
    private static readonly int HurtParam = Animator.StringToHash("hurt");

    void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        nyawaSebelumnya = nyawaMaksimum;
        animator = GetComponent<Animator>();

        if (healthBar != null)
        {
            healthBar.SetHealth(nyawaSekarang, nyawaMaksimum);
            healthBar.target = transform; // agar health bar ikut posisi bos
        }

        if (portalSaatBosMati != null)
        {
            portalSaatBosMati.SetActive(false);
        }
    }

    public void KurangiNyawa()
    {
        if (sudahMati) return;

        nyawaSekarang--;
        nyawaSekarang = Mathf.Max(0, nyawaSekarang);
        Debug.Log("Bos2 kena! Sisa nyawa: " + nyawaSekarang);

        // Perbaikan logika: drop item hati setiap nyawa berkurang kelipatan 10
        // Misalnya: 90, 80, 70, 60, 50, 40, 30, 20, 10
        if (nyawaSekarang > 0 && nyawaSekarang % 10 == 0)
        {
            DropItem(prefabHati);
            Debug.Log($"💖 Item hati dijatuhkan saat nyawa tersisa {nyawaSekarang}!");
        }

        nyawaSebelumnya = nyawaSekarang;

        if (animator != null)
        {
            animator.SetTrigger(HurtParam);
        }

        // 🩸 Update UI HealthBar
        if (healthBar != null)
        {
            healthBar.SetHealth(nyawaSekarang, nyawaMaksimum);
        }

        if (nyawaSekarang <= 0)
        {
            Mati();
        }
    }

    void DropItem(GameObject prefab, int jumlah = 1)
    {
        if (prefab == null) return;

        Vector3 basePos = transform.position + Vector3.down * 1.5f;

        for (int i = 0; i < jumlah; i++)
        {
            Vector3 posisiSpawn = basePos; // Bisa dikembangkan jadi berjejer
            Instantiate(prefab, posisiSpawn, Quaternion.identity);
            Debug.Log($"💖 Item dropped di posisi {posisiSpawn}");
        }
    }

    private void Mati()
    {
        sudahMati = true;
        Debug.Log("Bos2 mati");

        if (animator != null)
        {
            animator.SetBool("isDead", true);
            animator.SetTrigger(DieParam);
        }

        // Matikan collider dan physics
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

        if (portalSaatBosMati != null)
        {
            portalSaatBosMati.SetActive(true);
        }

        if (prefabCrystal != null)
        {
            Vector3 posisiSpawn = transform.position + Vector3.up * 1f;
            Instantiate(prefabCrystal, posisiSpawn, Quaternion.identity);
            Debug.Log("💎 Crystal dijatuhkan saat bos mati!");
        }

        Invoke(nameof(Hancurkan), 1.3f);
    }

    public void Hancurkan()
    {
        Destroy(gameObject);
    }
}
