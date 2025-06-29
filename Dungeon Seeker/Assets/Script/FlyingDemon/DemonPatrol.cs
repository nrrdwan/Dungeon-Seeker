using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemonPatrol : MonoBehaviour
{
    [Header("Patrol Settings (Relative Positions)")]
    [SerializeField] private Vector2[] relativePatrolPoints = {
        new Vector2(-2f, 0f),
        new Vector2(2f, 0f)
    };

    public float patrolSpeed = 2f;
    public float reachDistance = 0.2f;

    [Header("Player Detection Settings")]
    public float detectionRange = 3f;

    private int currentIndex = 0;
    private bool facingRight;
    private float initialScaleX;
    private Animator animator;
    private bool playerDetected = false;
    private AttackDemon demonAttack;

    private Vector2[] worldPatrolPoints;
    private Vector2 spawnPosition;

    void Start()
    {
        if (relativePatrolPoints == null || relativePatrolPoints.Length < 2)
        {
            Debug.LogWarning("❌ DemonPatrol: Minimal 2 titik posisi diperlukan!");
            enabled = false;
            return;
        }

        spawnPosition = transform.position;
        CalculateWorldPatrolPoints();

        initialScaleX = Mathf.Abs(transform.localScale.x);

        // ✅ SOLUSI BARU: Cari target terdekat sebagai starting point
        FindNearestPatrolPoint();

        // Tentukan arah berdasarkan target pertama dari posisi spawn
        Vector2 firstTarget = worldPatrolPoints[currentIndex];
        Vector2 directionToTarget = firstTarget - spawnPosition;
        facingRight = directionToTarget.x >= 0f;

        Vector3 scale = transform.localScale;
        scale.x = facingRight ? initialScaleX : -initialScaleX;
        transform.localScale = scale;

        animator = GetComponent<Animator>();
        demonAttack = GetComponent<AttackDemon>();

        Debug.Log($"👹 Demon spawn: {spawnPosition}, Target terdekat: {firstTarget} (index: {currentIndex}), facingRight: {facingRight}");
    }

    void FindNearestPatrolPoint()
    {
        float minDistance = float.MaxValue;
        int nearestIndex = 0;

        for (int i = 0; i < worldPatrolPoints.Length; i++)
        {
            float distance = Vector2.Distance(spawnPosition, worldPatrolPoints[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = i;
            }
        }

        currentIndex = nearestIndex;
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            playerDetected = distance <= detectionRange;
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

        if (distance < reachDistance)
        {
            int nextIndex = (currentIndex + 1) % worldPatrolPoints.Length;
            Vector2 nextTarget = worldPatrolPoints[nextIndex];
            Vector2 dir = nextTarget - worldPatrolPoints[currentIndex];

            if (!playerDetected)
            {
                if ((dir.x > 0 && !facingRight) || (dir.x < 0 && facingRight))
                {
                    Flip();
                }
            }

            currentIndex = nextIndex;
        }
    }

    void Flip()
    {
        if (playerDetected)
        {
            Debug.Log("⚠️ Flip dicegah karena player terdeteksi.");
            return;
        }

        Debug.Log("✅ Flip patrol dijalankan.");
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? initialScaleX : -initialScaleX;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    public bool IsPlayerDetected()
    {
        return playerDetected;
    }

    public void SetCustomPatrolPoints(Vector2[] customPoints)
    {
        relativePatrolPoints = customPoints;
        if (Application.isPlaying)
        {
            CalculateWorldPatrolPoints();
        }
    }
}
