using UnityEngine;

public class SistemNyawaBos : MonoBehaviour
{
    [Header("Nyawa")]
    public int nyawaMaksimum = 100;
    private int nyawaSekarang;
    private int nyawaSebelumnya;

    [Header("Komponen")]
    private Animator animator;
    private bool sudahMati = false;

    [Header("UI Health Bar")]
    public HealthBar healthBar;

    [Header("Prefab Item Drop")]
    public GameObject prefabHati;
    public GameObject prefabAnakSlime;
    public GameObject prefabCrystal; // 🔥 Crystal saat bos mati

    [Header("Portal")]
    public GameObject portalSaatBosMati;

    void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        nyawaSebelumnya = nyawaMaksimum;
        animator = GetComponent<Animator>();

        if (healthBar != null)
        {
            healthBar.SetHealth(nyawaSekarang, nyawaMaksimum);
            healthBar.target = transform;
        }

        // 🚪 Matikan portal di awal game
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

        Debug.Log("Bos kena! Sisa nyawa: " + nyawaSekarang);

        if (nyawaSekarang % 10 == 0 && nyawaSebelumnya % 10 != 0)
        {
            DropItem(prefabHati);
        }
        else if (nyawaSekarang % 5 == 0 && nyawaSebelumnya % 5 != 0)
        {
            DropItem(prefabAnakSlime, 2);
        }

        nyawaSebelumnya = nyawaSekarang;

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
        float jarakAntarAnak = 3f;

        for (int i = 0; i < jumlah; i++)
        {
            float xOffset = (i - (jumlah - 1) / 2f) * jarakAntarAnak;
            Vector3 posisiSpawn = basePos + new Vector3(xOffset, 0f, 0f);

            Instantiate(prefab, posisiSpawn, Quaternion.identity);
            Debug.Log($"🧬 Spawn anak ke-{i + 1} di posisi {posisiSpawn}");
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

        // ✅ Aktifkan portal
        if (portalSaatBosMati != null)
        {
            portalSaatBosMati.SetActive(true);
        }

        // 💎 Drop crystal di tengah bos
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
        Debug.Log("💥 Fungsi Hancurkan() dipanggil");

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        Destroy(gameObject);
    }
}
