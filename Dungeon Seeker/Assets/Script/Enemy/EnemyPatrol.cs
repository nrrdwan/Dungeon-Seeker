using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    private Rigidbody2D rb;
    private Transform target;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        target = pointB;
    }

    void Update()
    {
        Debug.Log("Moving to: " + target.name);
        Debug.Log("Enemy Pos: " + transform.position);
        Debug.Log("Target Pos: " + target.position);
        Debug.Log("Velocity: " + rb.velocity);
        // Gerak ke target
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * speed, rb.velocity.y);

        // Flip arah kalau udah deket
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            target = (target == pointA) ? pointB : pointA;

            // Flip visual sprite
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}
