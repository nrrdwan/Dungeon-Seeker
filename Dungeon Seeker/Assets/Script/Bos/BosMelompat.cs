using UnityEngine;

public class BosMelompat : MonoBehaviour
{
    public Transform targetPlayer;
    public Animator animator;
    public Rigidbody2D rb;

    public float jarakLompat = 7f;
    public float tinggiLompat = 6f;
    public float delayLompatan = 3f;
    public float delaySebelumLompatan = 1f;

    public int damageKePlayer = 1;

    private bool sedangMelompat = false;
    private bool sudahNabrak = false;
    private float arahLompatan = 1f;

    void Start()
    {
        if (targetPlayer == null)
            targetPlayer = GameObject.FindGameObjectWithTag("Player")?.transform;

        InvokeRepeating(nameof(SiapkanLompatan), delayLompatan, delayLompatan);
    }

    void SiapkanLompatan()
    {
        if (targetPlayer == null || rb == null) return;

        // ✅ Batalkan lompatan kalau bos masih di atas kepala player
        float jarakVertikal = transform.position.y - targetPlayer.position.y;
        float jarakHorizontal = Mathf.Abs(transform.position.x - targetPlayer.position.x);

        if (jarakVertikal > 0.5f && jarakHorizontal < 1f)
        {
            Debug.Log("❌ Bos sedang di atas player, tunda lompatan");
            return;
        }

        arahLompatan = Mathf.Sign(targetPlayer.position.x - transform.position.x);
        animator.SetBool("boslompat", true);
        sedangMelompat = true;
        sudahNabrak = false;

        Invoke(nameof(LompatKePlayer), delaySebelumLompatan);
    }

    void LompatKePlayer()
    {
        Vector2 gayaLompat = new Vector2(arahLompatan * jarakLompat, tinggiLompat);
        rb.velocity = Vector2.zero;
        rb.AddForce(gayaLompat, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && sedangMelompat && !sudahNabrak)
        {
            sudahNabrak = true;

            SistemNyawa nyawa = collision.gameObject.GetComponent<SistemNyawa>();
            if (nyawa != null)
            {
                nyawa.KurangiNyawa();
                Vector2 doronganMundur = new Vector2(-arahLompatan * 1f, 0.5f);
                rb.AddForce(doronganMundur, ForceMode2D.Impulse);
                rb.AddForce(Vector2.down * 30f, ForceMode2D.Impulse);
                Debug.Log("☠️ Player terlindas bos!");
            }

            // Agar tidak nyangkut di atas player
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider, true);

            // Balikin collider 1.5 detik kemudian (opsional)
            Invoke(nameof(ResetCollisionDenganPlayer), 1.5f);

            animator.SetBool("boslompat", false);
            sedangMelompat = false;
        }

        // Deteksi mendarat ke tanah
        if (collision.gameObject.CompareTag("Ground") || collision.contacts[0].normal.y > 0.5f)
        {
            animator.SetBool("boslompat", false);
            sedangMelompat = false;
        }
    }

    void ResetCollisionDenganPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player.GetComponent<Collider2D>(), false);
        }
    }
}
