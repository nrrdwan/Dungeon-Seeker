using UnityEngine;

public class LavaInstantKill : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah objek yang menyentuh lava adalah Player dan punya SistemNyawa
        SistemNyawa nyawa = other.GetComponent<SistemNyawa>();
        if (nyawa != null)
        {
            nyawa.InstanMati();
        }
    }
}
