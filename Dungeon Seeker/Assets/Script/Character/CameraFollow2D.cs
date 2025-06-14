using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target; // Objek yang diikuti, biasanya karakter player
    public Vector3 offset = new Vector3(0f, 1f, -10f); // Offset posisi kamera terhadap player
    public float smoothSpeed = 0.125f; // Seberapa halus pergerakan kamera

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
