using UnityEngine;

public class Lempar : MonoBehaviour
{
    public Transform player;
    public float throwForce = 10f;
    public float returnDelay = 1.5f;
    public float returnSpeed = 8f;

    private bool isReturning = false;
    private Rigidbody2D rb;
    private bool isThrown = false;

    public bool IsAvailable => !isThrown; // Bisa dilempar kalau belum dilempar

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (isThrown && isReturning)
        {
            Vector2 direction = ((Vector2)player.position - rb.position).normalized;
            rb.velocity = direction * returnSpeed;

            if (Vector2.Distance(rb.position, player.position) < 0.5f)
            {
                ResetThrowable();
            }
        }
    }

    public void Throw()
{
    if (isThrown) return; // Cegah lempar ulang jika belum selesai

    transform.position = player.position;
    gameObject.SetActive(true);
    isThrown = true;
    isReturning = false;

    float directionX = player.localScale.x >= 0 ? 1f : -1f;
    Vector2 direction = new Vector2(directionX, 0f);

    Vector3 newScale = transform.localScale;
    newScale.x = Mathf.Abs(newScale.x) * directionX;
    transform.localScale = newScale;

    rb.velocity = direction * throwForce;

    Invoke(nameof(StartReturn), returnDelay);
}

    void StartReturn()
    {
        isReturning = true;
    }

    void ResetThrowable()
    {
        gameObject.SetActive(false);
        rb.velocity = Vector2.zero;
        isThrown = false;
        isReturning = false;
        CancelInvoke();
    }
}
