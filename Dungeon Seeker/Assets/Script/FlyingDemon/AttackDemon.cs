using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDemon : MonoBehaviour
{
    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public Transform titikLempar;
    public float jarakLempar = 5f;
    public float kecepatanLempar = 5f;
    public float delayLempar = 2f;

    [Header("Projectile Settings")]
    public float projectileScale = 1f;
    public float projectileLifetime = 5f;

    private float waktuTerakhirLempar;
    private Transform targetPlayer;
    private Animator animator;

    // Tambahan baru:
    private bool allowAutoFlip = false;
    private float initialScaleX;

    private void Start()
    {
        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        initialScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void Update()
    {
        if (targetPlayer == null) return;

        float jarak = Vector2.Distance(transform.position, targetPlayer.position);

        // Flip hanya jika diizinkan
        if (allowAutoFlip)
        {
            FlipTowardsPlayer();
        }

        if (jarak <= jarakLempar && Time.time > waktuTerakhirLempar + delayLempar)
        {
            allowAutoFlip = true; // hanya flip saat akan menyerang
            LemparKePlayer();
            waktuTerakhirLempar = Time.time;
        }
        else
        {
            allowAutoFlip = false; // reset setelah lemparan
        }
    }

    void FlipTowardsPlayer()
    {
        if (targetPlayer.position.x > transform.position.x && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(initialScaleX, transform.localScale.y, transform.localScale.z);
        }
        else if (targetPlayer.position.x < transform.position.x && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-initialScaleX, transform.localScale.y, transform.localScale.z);
        }
    }

    void LemparKePlayer()
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }

        GameObject peluru = Instantiate(projectilePrefab, titikLempar.position, Quaternion.identity);
        Rigidbody2D rb = peluru.GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        peluru.transform.localScale = Vector3.one * projectileScale;

        Vector2 arah = (targetPlayer.position - titikLempar.position).normalized;
        rb.velocity = arah * kecepatanLempar;

        float angle = Mathf.Atan2(arah.y, arah.x) * Mathf.Rad2Deg;
        peluru.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        peluru.tag = "Penghalang";
        peluru.AddComponent<ProjectileCollision>();

        Destroy(peluru, projectileLifetime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, jarakLempar);

        if (titikLempar != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(titikLempar.position, 0.2f);

            if (targetPlayer != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(titikLempar.position, targetPlayer.position);
            }
        }
    }
}

public class ProjectileCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
