using UnityEngine;

public class Lempar : MonoBehaviour
{
    public Transform player;
    public float throwForce = 10f;
    public float disappearAfter = 2f; // waktu setelah dilempar sebelum menghilang

    private Rigidbody2D rb;
    private bool isThrown = false;

    public bool IsAvailable => !isThrown; // Bisa dilempar kalau belum dilempar

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameObject.SetActive(false);
    }

    public void Throw()
    {
        if (isThrown) return; // Cegah lempar ulang jika belum selesai

        transform.position = player.position;
        gameObject.SetActive(true);
        isThrown = true;

        float directionX = player.localScale.x >= 0 ? 1f : -1f;
        Vector2 direction = new Vector2(directionX, 0f);

        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Abs(newScale.x) * directionX;
        transform.localScale = newScale;

        rb.velocity = direction * throwForce;

        Invoke(nameof(DisableAfterTime), disappearAfter); // hilang setelah waktu tertentu
    }

    void DisableAfterTime()
    {
        ResetThrowable();
    }

    void ResetThrowable()
    {
        gameObject.SetActive(false);
        rb.velocity = Vector2.zero;
        isThrown = false;
        CancelInvoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mob"))
        {
            SistemNyawaMob nyawaMob = other.GetComponent<SistemNyawaMob>();
            if (nyawaMob != null)
            {
                nyawaMob.KurangiNyawa();
            }

            // Langsung hilang setelah kena musuh
            ResetThrowable();
        }
    }
}
