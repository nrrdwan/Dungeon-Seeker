using System.Collections;
using System.Collections.Generic;
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

    [Header("Prefab Item Drop")]
    public GameObject prefabHati;
    public GameObject prefabCrystal; // 🔥 Crystal saat bos mati

    [Header("Portal")]
    public GameObject portalSaatBosMati;

    private static readonly int DieParam = Animator.StringToHash("die");
    private static readonly int HurtParam = Animator.StringToHash("hurt");

    void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        nyawaSebelumnya = nyawaMaksimum;
        animator = GetComponent<Animator>();

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
        Debug.Log("Bos2 kena! Sisa nyawa: " + nyawaSekarang);

        // Drop heart setiap 10 nyawa berkurang
        if (nyawaSekarang % 10 == 0 && nyawaSebelumnya % 10 != 0)
        {
            DropItem(prefabHati);
        }

        nyawaSebelumnya = nyawaSekarang;

        if (animator != null)
        {
            animator.SetTrigger("hurt");
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
            Vector3 posisiSpawn = basePos + new Vector3(0f, 0f, 0f);
            Instantiate(prefab, posisiSpawn, Quaternion.identity);
            Debug.Log($"💖 Heart dropped di posisi {posisiSpawn}");
        }
    }

    private void Mati()
    {
        sudahMati = true;
        Debug.Log("Bos2 mati");

        if (animator != null)
        {
            animator.SetBool("isDead", true);
            animator.SetTrigger("die");
        }

        // Matikan physics dan collider
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }

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
    }

    // Fungsi ini akan dipanggil lewat Animation Event di akhir animasi mati
    public void Hancurkan()
    {
        Destroy(gameObject);
    }
}
