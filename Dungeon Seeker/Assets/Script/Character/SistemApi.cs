using UnityEngine;

public class SistemApi : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private float fireDamageInterval = 1f; // Damage tiap berapa detik
    [SerializeField] private float sinkSpeed = 0.5f;
    [SerializeField] private float maxSinkDepth = 0.5f;

    private float damageTimer = 0f;
    private bool isInFire = false;
    private float currentSinkDepth = 0f;
    private Vector3 originalPosition;

    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;
    private Rigidbody2D rb;
    private float originalGravity;

    // 🔗 Referensi ke SistemNyawa
    private SistemNyawa sistemNyawa;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            originalGravity = rb.gravityScale;
        }

        originalPosition = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        sistemNyawa = GetComponent<SistemNyawa>();
        if (sistemNyawa == null)
        {
            Debug.LogError("❌ SistemNyawa tidak ditemukan di GameObject ini!");
        }
    }

    private void Update()
    {
        if (isInFire)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= fireDamageInterval)
            {
                KurangiNyawa(); // 🔥 Damage dari api
                damageTimer = 0f;
            }

            if (currentSinkDepth < maxSinkDepth)
            {
                currentSinkDepth += sinkSpeed * Time.deltaTime;
                Vector3 newPos = transform.position;
                newPos.y -= sinkSpeed * Time.deltaTime;
                transform.position = newPos;
            }
        }
        else
        {
            if (currentSinkDepth > 0)
            {
                currentSinkDepth -= sinkSpeed * Time.deltaTime;
                Vector3 newPos = transform.position;
                newPos.y += sinkSpeed * Time.deltaTime;
                transform.position = newPos;

                if (currentSinkDepth <= 0)
                {
                    currentSinkDepth = 0;
                    Vector3 resetPos = transform.position;
                    resetPos.y = originalPosition.y;
                    transform.position = resetPos;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fire"))
        {
            isInFire = true;
            originalPosition = transform.position;

            if (rb != null) rb.gravityScale = 0;
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = originalSortingOrder - 1;

            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("burn"); // Optional animasi terbakar
            }

            Debug.Log("🔥 Masuk ke api!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Fire"))
        {
            isInFire = false;

            if (rb != null) rb.gravityScale = originalGravity;
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = originalSortingOrder;

            Debug.Log("💨 Keluar dari api!");
        }
    }

    private void KurangiNyawa(int jumlah = 1)
    {
        if (sistemNyawa != null)
        {
            for (int i = 0; i < jumlah; i++)
            {
                sistemNyawa.KurangiNyawa();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Tidak bisa kurangi nyawa, SistemNyawa tidak ditemukan!");
        }
    }
}
