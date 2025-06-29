using UnityEngine;

public class Lempar : MonoBehaviour
{
    public Transform player;
    public float throwForce = 10f;
    public float disappearAfter = 2f;
    public float offset = 0.5f;

    private Rigidbody2D rb;
    private bool isThrown = false;
    private float currentDirectionX = 1f; // arah lempar yang dikunci saat awal lempar

    public bool IsAvailable => !isThrown;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.SetActive(false);
    }

    public void Throw()
    {
        if (isThrown) return;

        // 🔒 Kunci arah lempar berdasarkan arah player saat tombol ditekan
        currentDirectionX = player.localScale.x >= 0 ? 1f : -1f;

        isThrown = true;
        gameObject.SetActive(true);

        Vector2 direction = new Vector2(currentDirectionX, 0f);

        // ⏩ Lempar dari sisi player ke arah yang terkunci
        transform.position = player.position + new Vector3(offset * currentDirectionX, 0f, 0f);

        // Atur skala agar objek terlihat hadap ke arah lemparan
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * currentDirectionX;
        transform.localScale = newScale;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.velocity = direction * throwForce;

        Invoke(nameof(DisableAfterTime), disappearAfter);
    }

    void DisableAfterTime()
    {
        ResetThrowable();
    }

    void ResetThrowable()
    {
        CancelInvoke();
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isThrown = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mob"))
        {
            bool hitProcessed = false;

            // Cek HealthFlyingDemon terlebih dahulu (prioritas)
            HealthFlyingDemon healthFlyingDemon = other.GetComponent<HealthFlyingDemon>();
            if (healthFlyingDemon != null)
            {
                healthFlyingDemon.KurangiNyawa();
                hitProcessed = true;
            }
            
            // Jika bukan Flying Demon, cek mob biasa
            if (!hitProcessed)
            {
                SistemNyawaMob nyawaMob = other.GetComponent<SistemNyawaMob>();
                if (nyawaMob != null)
                {
                    nyawaMob.KurangiNyawa();
                    hitProcessed = true;
                }
            }

            if (hitProcessed)
            {
                ResetThrowable();
            }
        }

        // Untuk bos
        if (other.CompareTag("Bos"))
        {
            HealthBoss2 nyawaBos = other.GetComponent<HealthBoss2>();
            if (nyawaBos != null)
            {
                nyawaBos.KurangiNyawa();
            }

            ResetThrowable();
        }
    }
}
