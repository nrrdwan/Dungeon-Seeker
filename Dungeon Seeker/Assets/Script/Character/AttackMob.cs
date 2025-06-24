using UnityEngine;

public class AttackMob : MonoBehaviour
{
    private SistemNyawaMob mobTerdekat;

    // Simpan mob yang sedang bersentuhan
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Mob"))
        {
            mobTerdekat = other.GetComponent<SistemNyawaMob>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Mob"))
        {
            if (mobTerdekat == other.GetComponent<SistemNyawaMob>())
            {
                mobTerdekat = null;
            }
        }
    }

    // Panggil fungsi ini saat serangan melee (misal dari PlayerMovement)
    public void TryHitMob()
    {
        if (mobTerdekat != null)
        {
            mobTerdekat.KurangiNyawa();
        }
    }
}
