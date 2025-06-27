using UnityEngine;

public class SistemNyawaBos : MonoBehaviour
{
    [Header("Nyawa")]
    public int nyawaMaksimum = 100;
    private int nyawaSekarang;

    [Header("Komponen")]
    private Animator animator;
    private bool sudahMati = false;

    [Header("UI Health Bar")]
    public HealthBar healthBar;

    void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        animator = GetComponent<Animator>();

        if (healthBar != null)
        {
            healthBar.SetHealth(nyawaSekarang, nyawaMaksimum);
            healthBar.target = transform;
        }
    }

    public void KurangiNyawa()
    {
        if (sudahMati) return;

        nyawaSekarang--;
        nyawaSekarang = Mathf.Max(0, nyawaSekarang);
        Debug.Log("Bos kena! Sisa nyawa: " + nyawaSekarang);

        if (healthBar != null)
        {
            healthBar.SetHealth(nyawaSekarang, nyawaMaksimum);
        }

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
