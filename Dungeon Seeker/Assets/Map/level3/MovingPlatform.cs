using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform platform;
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 2f;

    private Vector3 target;
    private Vector3 lastPlatformPosition;

    void Start()
    {
        target = endPoint.position;
        lastPlatformPosition = platform.position;
    }

    void Update()
    {
        // Gerakkan platform
        platform.position = Vector3.MoveTowards(platform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(platform.position, target) < 0.01f)
        {
            target = (target == startPoint.position) ? endPoint.position : startPoint.position;
        }
    }

    void FixedUpdate()
    {
        lastPlatformPosition = platform.position;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Vector3 platformDelta = platform.position - lastPlatformPosition;

            // Gerakkan player menggunakan Rigidbody agar tidak ditimpa movement script
            Rigidbody2D playerRb = collision.collider.GetComponent<Rigidbody2D>();
            if (playerRb != null && platformDelta != Vector3.zero)
            {
                playerRb.MovePosition(playerRb.position + (Vector2)platformDelta);
            }
        }
    }
}
