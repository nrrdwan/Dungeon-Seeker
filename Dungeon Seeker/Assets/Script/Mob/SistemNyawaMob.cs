using UnityEngine;

public class SistemNyawaMob : MonoBehaviour
{
    public int nyawaMaksimum = 2;
    private int nyawaSekarang;

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
        animator.SetBool("isDead", true);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;

        // Hancurkan setelah animasi selesai (ganti sesuai durasi animasi)
        Destroy(gameObject, 2f); 
    }
}
