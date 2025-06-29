using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings (Relative Positions)")]
    [SerializeField] private Vector2[] relativePatrolPoints = { 
        new Vector2(-2f, 0f), 
        new Vector2(2f, 0f) 
    }; // Posisi relatif dari enemy spawn point
    
    public float patrolSpeed = 2f;
    public float reachDistance = 0.2f;

    [Header("Player Detection Settings")]
    public float detectionRange = 3f;
    public LayerMask playerLayer = 1;
    
    private int currentIndex = 0;
    private bool facingRight = true;
    private float initialScaleX;
    private Animator animator;
    private bool playerDetected = false;
    private Enemy enemyAttack;
    
    // World positions calculated from relative positions
    private Vector2[] worldPatrolPoints;
    private Vector2 spawnPosition;

    void Start()
    {
        if (relativePatrolPoints == null || relativePatrolPoints.Length < 2)
        {
            Debug.LogWarning("❌ EnemyPatrol: Minimal 2 titik posisi diperlukan!");
            enabled = false;
            return;
        }

        // Save spawn position and calculate world patrol points
        spawnPosition = transform.position;
        CalculateWorldPatrolPoints();

        initialScaleX = transform.localScale.x;
        animator = GetComponent<Animator>();
        enemyAttack = GetComponent<Enemy>();
    }

    void CalculateWorldPatrolPoints()
    {
        worldPatrolPoints = new Vector2[relativePatrolPoints.Length];
        for (int i = 0; i < relativePatrolPoints.Length; i++)
        {
            worldPatrolPoints[i] = spawnPosition + relativePatrolPoints[i];
        }
    }

    void Update()
    {
        DetectPlayer();
        
        if (!playerDetected)
        {
            Patrol();
        }
        else
        {
            if (animator != null)
                animator.SetBool("moving", false);
        }
    }

    void DetectPlayer()
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position, 
            detectionRange, 
            Vector2.zero, 
            0f, 
            playerLayer
        );

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            playerDetected = true;
            
            Vector2 playerDirection = hit.collider.transform.position - transform.position;
            if ((playerDirection.x > 0 && !facingRight) || (playerDirection.x < 0 && facingRight))
            {
                Flip();
            }
        }
        else
        {
            playerDetected = false;
        }
    }

    void Patrol()
    {
        Vector2 target = worldPatrolPoints[currentIndex];
        Vector2 currentPos = transform.position;

        float distance = Vector2.Distance(currentPos, target);

        if (animator != null)
            animator.SetBool("moving", distance > reachDistance * 0.5f);

        transform.position = Vector2.MoveTowards(currentPos, target, patrolSpeed * Time.deltaTime);

        Vector2 dir = target - currentPos;
        if ((dir.x > 0 && !facingRight) || (dir.x < 0 && facingRight))
        {
            Flip();
        }

        if (distance < reachDistance)
        {
            currentIndex = (currentIndex + 1) % worldPatrolPoints.Length;
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
        // Show relative patrol points in edit mode
        if (!Application.isPlaying)
        {
            Vector2 currentPos = transform.position;
            Gizmos.color = Color.cyan;
            
            for (int i = 0; i < relativePatrolPoints.Length; i++)
            {
                Vector2 worldPos = currentPos + relativePatrolPoints[i];
                Gizmos.DrawSphere(worldPos, 0.2f);

                if (i + 1 < relativePatrolPoints.Length)
                {
                    Vector2 nextWorldPos = currentPos + relativePatrolPoints[i + 1];
                    Gizmos.DrawLine(worldPos, nextWorldPos);
                }
            }
        }
        // Show calculated world points in play mode
        else if (worldPatrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < worldPatrolPoints.Length; i++)
            {
                Gizmos.DrawSphere(worldPatrolPoints[i], 0.2f);

                if (i + 1 < worldPatrolPoints.Length)
                    Gizmos.DrawLine(worldPatrolPoints[i], worldPatrolPoints[i + 1]);
            }
        }

        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    public bool IsPlayerDetected()
    {
        return playerDetected;
    }

    // Optional: Method to customize patrol points per instance
    public void SetCustomPatrolPoints(Vector2[] customPoints)
    {
        relativePatrolPoints = customPoints;
        if (Application.isPlaying)
        {
            CalculateWorldPatrolPoints();
        }
    }
}
