using UnityEngine;

public class PlayerStatTracker : MonoBehaviour
{
    [Header("Statistik Player")]
    public int totalPoin = 0;
    public int totalMob = 0;

    public void TambahPoin(int jumlah)
    {
        totalPoin += jumlah;
    }

    public void TambahMob()
    {
        totalMob++;
    }
}
