using UnityEngine;

public class SistemApi : MonoBehaviour
{
    [Header("Nyawa")]
    public int nyawaMaksimum = 3;
    private int nyawaSekarang;
    public GameObject[] ikonNyawa;
    public GameObject panelGameOver;

    [Header("Api")]
    [SerializeField] private float fireDamageInterval = 1f;
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

    void Start()
    {
        nyawaSekarang = nyawaMaksimum;
        UpdateUI();

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
    }

    void Update()
    {
        if (isInFire)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= fireDamageInterval)
            {
                KurangiNyawa(); // Damage dari api
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

    private void KurangiNyawa(int jumlah = 1)
    {
        nyawaSekarang -= jumlah;
        Debug.Log("Nyawa berkurang: " + jumlah + " | Sisa nyawa: " + nyawaSekarang);
        UpdateUI();

        if (nyawaSekarang <= 0)
        {
            GameOver();
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < ikonNyawa.Length; i++)
        {
            ikonNyawa[i].SetActive(i < nyawaSekarang);
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }

        // Freeze player movement (opsional)
        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        Time.timeScale = 0f; // Pause game
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fire"))
{
    isInFire = true;
    originalPosition = transform.position;

    // 🔽 Matikan gravitasi agar tenggelam pelan bisa terlihat
    if (rb != null) rb.gravityScale = 0;

    if (spriteRenderer != null)
        spriteRenderer.sortingOrder = originalSortingOrder - 1;
}
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Fire"))
{
    isInFire = false;

    // 🔼 Kembalikan gravitasi seperti semula
    if (rb != null) rb.gravityScale = originalGravity;

    if (spriteRenderer != null)
        spriteRenderer.sortingOrder = originalSortingOrder;
}
    }
}
