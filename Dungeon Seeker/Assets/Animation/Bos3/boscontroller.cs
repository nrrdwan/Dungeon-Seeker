using UnityEngine;

public class BossController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float attackRange = 2f;     // jarak untuk mulai serang
    public float chaseRange = 6f;      // jarak bos masih mau ngejar cepat
    public float visionRange = 12f;    // jarak maksimum bos masih bisa melihat player
    public float attackCooldown = 1.5f;

    private Transform player;
    private Animator animator;
    private bool isFacingRight = true;
    private bool isAttacking = false;

    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Flip ke arah player
        if (player.position.x < transform.position.x && isFacingRight)
        {
            Flip();
        }
        else if (player.position.x > transform.position.x && !isFacingRight)
        {
            Flip();
        }

        if (distanceToPlayer <= attackRange)
        {
            // Player sangat dekat → berhenti & serang
            animator.SetBool("isWalking", false);

            if (!isAttacking)
            {
                isAttacking = true;
                animator.SetTrigger("Attack");
                Invoke(nameof(ResetAttack), attackCooldown);
            }
        }
        else if (distanceToPlayer <= visionRange)
        {
            // Player masih terlihat (jauh) → jalan ke arah player
            animator.SetBool("isWalking", true);

            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            // Player terlalu jauh (hilang dari penglihatan) → diam
            animator.SetBool("isWalking", false);
        }
    }

    void FixedUpdate()
    {
        // Biar bos tidak bisa didorong player
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
