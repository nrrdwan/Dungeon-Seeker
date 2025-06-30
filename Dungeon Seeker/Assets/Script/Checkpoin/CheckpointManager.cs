using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static Vector3 posisiCheckpointTerakhir;

    private void Awake()
    {
        // Jika kamu punya GameObject "SpawnPoint" sebagai posisi awal
        GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawn != null)
        {
            posisiCheckpointTerakhir = spawn.transform.position;
        }
        else
        {
            posisiCheckpointTerakhir = Vector3.zero;
        }
    }

    public static void SetCheckpoint(Vector3 posisiBaru)
    {
        posisiCheckpointTerakhir = posisiBaru;
        Debug.Log("✔️ Checkpoint disimpan di: " + posisiBaru);
    }

    public static Vector3 GetCheckpoint()
    {
        return posisiCheckpointTerakhir;
    }
}
