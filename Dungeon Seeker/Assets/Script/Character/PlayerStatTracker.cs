using UnityEngine;

public class PlayerStatTracker : MonoBehaviour
{
    public static PlayerStatTracker Instance;

    [Header("Statistik Player")]
    public int totalPoin = 0;
    public int totalMob = 0;

    [Header("Upgrade Cooldown")]
    public float dodgeCooldown = 1.5f;
    public float throwCooldown = 1.2f;
    public float attackCooldown = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Tetap hidup antar scene
        }
        else
        {
            Destroy(gameObject); // Hancurkan duplikat
        }
    }

    public void TambahPoin(int jumlah)
    {
        totalPoin += jumlah;
    }

    public void TambahMob()
    {
        totalMob++;
    }

    public void ApplyUpgrade(string type, float reduction)
    {
        switch (type)
        {
            case "dodge":
                dodgeCooldown = Mathf.Max(0.1f, dodgeCooldown - reduction);
                break;
            case "throw":
                throwCooldown = Mathf.Max(0.1f, throwCooldown - reduction);
                break;
            case "attack":
                attackCooldown = Mathf.Max(0.1f, attackCooldown - reduction);
                break;
        }
    }
}
