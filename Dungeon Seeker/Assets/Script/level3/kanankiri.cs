using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kanankiri : MonoBehaviour
{
    public Transform platform;
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 1.5f;

    int Direction = 1; // 1 for right, -1 for left

    private void Update()
    {
        Vector2 target = currentMovementTarget();
        platform.position = Vector2.MoveTowards(platform.position, target, speed * Time.deltaTime);

        // Jika sudah sangat dekat dengan target, balik arah
        if (Vector2.Distance(platform.position, target) < 0.05f)
        {
            Direction *= -1;
        }
    }

    Vector2 currentMovementTarget()
    {
    if (Direction == 1)
    {
        return startPoint.position;
    }
    else
    {
        return endPoint.position;
    }

    }

    private void OnDrawGizmos()
    {
        if (platform != null && startPoint != null && endPoint != null)
        {
            Gizmos.DrawLine(platform.transform.position, startPoint.position);
            Gizmos.DrawLine(platform.transform.position, endPoint.position);
        }
    }
}
