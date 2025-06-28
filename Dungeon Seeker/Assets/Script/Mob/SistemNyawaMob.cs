using UnityEngine;

public class SistemNyawaMob : MonoBehaviour
{
    public int nyawaMaksimum = 2;
    private int nyawaSekarang;

    private Animator animator;
    private bool sudahMati = false;

    // Tambahkan di bawah deklarasi variabel lain
    private static readonly int DieParam = Animator.StringToHash("die");
    private static readonly int HurtParam = Animator.StringToHash("hurt");

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

        animator.SetTrigger("hurt"); // Tambahan: trigger animasi hurt

        if (nyawaSekarang <= 0)
        {
            Mati();
        }
    }

    private void Mati()
    {
        sudahMati = true;
        Debug.Log("Mob mati");
        animator.SetBool("isDead", true);
        animator.SetTrigger("die");
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;

        // Hancurkan setelah animasi selesai (ganti sesuai durasi animasi)
        Destroy(gameObject, 2f); 
    }
}
