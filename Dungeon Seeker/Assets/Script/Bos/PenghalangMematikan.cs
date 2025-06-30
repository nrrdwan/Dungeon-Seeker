using UnityEngine;

public class PenghalangMematikan : MonoBehaviour
{
    [Header("Damage ke Player")]
    public int damage = 3;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Ambil komponen nyawa player
            var nyawa = collision.gameObject.GetComponent<SistemNyawa>();
            if (nyawa != null)
            {
                for (int i = 0; i < damage; i++)
                {
                    nyawa.KurangiNyawa();
                }

                Debug.Log("☠️ Player terkena penghalang dan kehilangan 3 nyawa.");
            }
        }
    }
}
