using UnityEngine;

public class MobPatrol : MonoBehaviour
{
    public float speed = 2f;
    public float playerDetectionRange = 5f;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private bool movingRight = true;
    private bool playerDetected = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); // PENTING

        InvokeRepeating("ToggleDirection", 2f, 2f);
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        playerDetected = distanceToPlayer <= playerDetectionRange;

        if (playerDetected)
        {
            FlipToPlayer();
            rb.velocity = Vector2.zero; // Diam
        }
        else
        {
            Move();
        }
    }

    void Move()
    {
        float direction = movingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * speed, rb.velocity.y);
        spriteRenderer.flipX = !movingRight;
    }

    void ToggleDirection()
    {
        if (!playerDetected)
        {
            movingRight = !movingRight;
        }
    }

    void FlipToPlayer()
    {
        if (player.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}
    