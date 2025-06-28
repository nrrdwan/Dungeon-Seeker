using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;     // Titik-titik patrol
    public float patrolSpeed = 2f;       // Kecepatan boss
    public float reachDistance = 0.2f;   // Seberapa dekat untuk pindah ke titik selanjutnya

    private int currentPointIndex = 0;   // Index titik sekarang
    private bool facingRight = true;
    private float initialScaleX;

    void Start()
    {
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            Debug.LogWarning("⚠️ BossPatrol: Butuh minimal 2 patrol point!");
            enabled = false;
            return;
        }

        initialScaleX = transform.localScale.x;
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        Transform target = patrolPoints[currentPointIndex];

        // Gerak ke arah target
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            patrolSpeed * Time.deltaTime
        );

        // Hitung arah untuk flip
        Vector3 direction = target.position - transform.position;
        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            Flip();
        }

        // Ganti titik kalau sudah cukup dekat
        if (Vector3.Distance(transform.position, target.position) < reachDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            Debug.Log("🔁 Boss sampai di titik " + currentPointIndex);
        }
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
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);

                    if (i + 1 < patrolPoints.Length && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }
            }
        }
    }
}
