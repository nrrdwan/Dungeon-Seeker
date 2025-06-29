using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public float[] patrolOffsetsX;        // Offset X dari posisi awal boss
    public float patrolSpeed = 2f;
    public float reachDistance = 0.2f;

    [Header("Animator")]
    public Animator animator; // Drag Animator di Inspector

    [Header("Player Detection")]
    public float detectDistance = 1f; // Jarak deteksi ke depan
    public float raycastYOffset = 0f; // Offset Y dari posisi boss
    public float raycastRadius = 0.5f; // Radius deteksi player

    private Vector3[] patrolPoints;
    private int currentPointIndex = 0;
    private bool facingRight = true;
    private float initialScaleX;
    private Vector3 startPosition;

    void Start()
    {
        if (patrolOffsetsX == null || patrolOffsetsX.Length < 2)
        {
            Debug.LogWarning("⚠️ BossPatrol: Butuh minimal 2 offset!");
            enabled = false;
            return;
        }

        startPosition = transform.position;
        patrolPoints = new Vector3[patrolOffsetsX.Length];
        for (int i = 0; i < patrolOffsetsX.Length; i++)
        {
            patrolPoints[i] = startPosition + new Vector3(patrolOffsetsX[i], 0, 0);
        }

        initialScaleX = transform.localScale.x;
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        // Deteksi player di depan
        Vector2 directionRayFront = facingRight ? Vector2.right : Vector2.left;
        Vector2 originFront = (Vector2)transform.position + directionRayFront * detectDistance * 0.5f;
        originFront.y += raycastYOffset;

        Collider2D hitFront = Physics2D.OverlapCircle(originFront, raycastRadius);
        Debug.DrawLine(transform.position, (Vector3)originFront, Color.red);

        // Deteksi player di belakang
        Vector2 directionRayBack = facingRight ? Vector2.left : Vector2.right;
        Vector2 originBack = (Vector2)transform.position + directionRayBack * detectDistance * 0.5f;
        originBack.y += raycastYOffset;

        Collider2D hitBack = Physics2D.OverlapCircle(originBack, raycastRadius);
        Debug.DrawLine(transform.position, (Vector3)originBack, Color.yellow);

        // Jika ada player di depan, berhenti
        if (hitFront != null && hitFront.CompareTag("Player"))
        {
            if (animator != null)
                animator.SetBool("moving", false);
            Debug.Log("🚨 Boss berhenti karena ada player di depan!");
            return;
        }

        // Jika ada player di belakang, flip dan berhenti
        if (hitBack != null && hitBack.CompareTag("Player"))
        {
            if (animator != null)
                animator.SetBool("moving", false);
            Flip();
            
            // Setelah flip, update target patrol ke arah yang baru
            UpdatePatrolDirectionAfterFlip();
            
            Debug.Log("🚨 Boss flip karena ada player di belakang!");
            return;
        }

        Vector3 target = patrolPoints[currentPointIndex];

        // Cek apakah boss sedang bergerak
        bool isMoving = Vector3.Distance(transform.position, target) > reachDistance;
        if (animator != null)
            animator.SetBool("moving", isMoving);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            patrolSpeed * Time.deltaTime
        );

        Vector3 direction = target - transform.position;
        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            Flip();
        }

        if (Vector3.Distance(transform.position, target) < reachDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            Debug.Log("🔁 Boss sampai di titik " + currentPointIndex);
        }
    }

    void UpdatePatrolDirectionAfterFlip()
    {
        // Cari patrol point terdekat yang searah dengan arah flip
        float currentX = transform.position.x;
        int bestIndex = currentPointIndex;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Vector3 point = patrolPoints[i];
            
            // Cek apakah point ini searah dengan arah flip
            bool pointIsRight = point.x > currentX;
            if (pointIsRight == facingRight)
            {
                float distance = Vector3.Distance(transform.position, point);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }
        }

        currentPointIndex = bestIndex;
        Debug.Log("🎯 Update patrol ke titik " + currentPointIndex + " setelah flip");
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x = initialScaleX * (facingRight ? 1 : -1);
        transform.localScale = scale;

        Debug.Log("🔄 Boss flip! Sekarang ngadep " + (facingRight ? "kanan" : "kiri"));
    }

    void OnDrawGizmosSelected()
    {
        if (patrolOffsetsX != null && patrolOffsetsX.Length > 0)
        {
            Gizmos.color = Color.green;
            Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
            for (int i = 0; i < patrolOffsetsX.Length; i++)
            {
                Vector3 point = startPos + new Vector3(patrolOffsetsX[i], 0, 0);
                Gizmos.DrawSphere(point, 0.3f);

                if (i + 1 < patrolOffsetsX.Length)
                {
                    Vector3 nextPoint = startPos + new Vector3(patrolOffsetsX[i + 1], 0, 0);
                    Gizmos.DrawLine(point, nextPoint);
                }
            }
        }

        // Gizmo untuk ray deteksi depan
        Gizmos.color = Color.red;
        Vector3 dirFront = (facingRight ? Vector3.right : Vector3.left) * detectDistance;
        Gizmos.DrawLine(transform.position, transform.position + dirFront);

        // Gizmo untuk ray deteksi belakang
        Gizmos.color = Color.yellow;
        Vector3 dirBack = (facingRight ? Vector3.left : Vector3.right) * detectDistance;
        Gizmos.DrawLine(transform.position, transform.position + dirBack);

        // Gizmo untuk area deteksi player depan
        Vector2 directionRayFront = facingRight ? Vector2.right : Vector2.left;
        Vector2 originFront = (Vector2)transform.position + directionRayFront * detectDistance * 0.5f;
        originFront.y += raycastYOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(originFront, raycastRadius);

        // Gizmo untuk area deteksi player belakang
        Vector2 directionRayBack = facingRight ? Vector2.left : Vector2.right;
        Vector2 originBack = (Vector2)transform.position + directionRayBack * detectDistance * 0.5f;
        originBack.y += raycastYOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(originBack, raycastRadius);
    }
}
