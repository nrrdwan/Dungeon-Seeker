using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings (Positions)")]
    public Vector2[] patrolPositions;    // Titik-titik world position
    public float patrolSpeed = 2f;
    public float reachDistance = 0.2f;

    private int currentIndex = 0;
    private bool facingRight = true;
    private float initialScaleX;

    void Start()
    {
        if (patrolPositions == null || patrolPositions.Length < 2)
        {
            Debug.LogWarning("❌ BossPatrol: Minimal 2 titik posisi diperlukan!");
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
        Vector2 target = patrolPositions[currentIndex];
        Vector2 currentPos = transform.position;

        transform.position = Vector2.MoveTowards(currentPos, target, patrolSpeed * Time.deltaTime);

        Vector2 dir = target - currentPos;
        if ((dir.x > 0 && !facingRight) || (dir.x < 0 && facingRight))
        {
            Flip();
        }

        if (Vector2.Distance(currentPos, target) < reachDistance)
        {
            currentIndex = (currentIndex + 1) % patrolPositions.Length;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x = initialScaleX * (facingRight ? 1 : -1);
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (patrolPositions != null && patrolPositions.Length > 0)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < patrolPositions.Length; i++)
            {
                Gizmos.DrawSphere(patrolPositions[i], 0.2f);

                if (i + 1 < patrolPositions.Length)
                    Gizmos.DrawLine(patrolPositions[i], patrolPositions[i + 1]);
            }
        }
    }
}
