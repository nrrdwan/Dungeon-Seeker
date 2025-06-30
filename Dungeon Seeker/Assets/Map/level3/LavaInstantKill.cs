using UnityEngine;

public class LavaInstantKill : MonoBehaviour
{
    [Header("Lava Damage Settings")]
    [SerializeField] private float lavaDamageInterval = 1f; // Damage tiap berapa detik
    [SerializeField] private int damagePerTick = 10; // Damage yang diberikan setiap tick
    [SerializeField] private float sinkSpeed = 0.5f;
    [SerializeField] private float maxSinkDepth = 0.5f;

    private float damageTimer = 0f;
    private bool isInLava = false;
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
        if (isInLava)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= lavaDamageInterval)
            {
                KurangiNyawa(damagePerTick); // 🌋 Damage dari lava
                damageTimer = 0f;
            }

            // Efek tenggelam di lava
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
            // Kembali ke posisi normal saat keluar dari lava
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang menyentuh lava punya SistemNyawa
        SistemNyawa nyawa = other.GetComponent<SistemNyawa>();
        if (nyawa != null)
        {
            isInLava = true;
            originalPosition = other.transform.position;

            // Set gravity dan sorting order
            Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
            if (otherRb != null) otherRb.gravityScale = 0;
            
            SpriteRenderer otherSprite = other.GetComponent<SpriteRenderer>();
            if (otherSprite != null)
                otherSprite.sortingOrder = otherSprite.sortingOrder - 1;

            // Optional animasi terbakar
            Animator anim = other.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("burn");
            }

            Debug.Log("🌋 Masuk ke lava!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Hentikan damage saat keluar dari lava
        SistemNyawa nyawa = other.GetComponent<SistemNyawa>();
        if (nyawa != null)
        {
            isInLava = false;

            // Reset gravity dan sorting order
            Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
            if (otherRb != null) otherRb.gravityScale = originalGravity;
            
            SpriteRenderer otherSprite = other.GetComponent<SpriteRenderer>();
            if (otherSprite != null)
                otherSprite.sortingOrder = originalSortingOrder;

            Debug.Log("💨 Keluar dari lava!");
        }
    }

    private void KurangiNyawa(int jumlah = 1)
    {
        if (sistemNyawa != null)
        {
            // Karena SistemNyawa.KurangiNyawa() tidak menerima parameter,
            // kita panggil berkali-kali sesuai jumlah damage
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
