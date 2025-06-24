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

    void Start()
    {
        // Awalnya bergerak menuju endPoint
        target = endPoint.position;
    }

    void Update()
    {
        // Gerakkan platform ke arah target
        platform.position = Vector3.MoveTowards(platform.position, target, speed * Time.deltaTime);

        // Jika sudah sampai tujuan, ganti target ke titik sebaliknya
        if (Vector3.Distance(platform.position, target) < 0.01f)
        {
            target = (target == startPoint.position) ? endPoint.position : startPoint.position;
        }
    }
}
